using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ogani_master.configs;
using ogani_master.enums;
using ogani_master.Models;
using ogani_master.Payments.dto;

namespace ogani_master.Payments.momo
{

    
    public class MomoService
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly ILogger<MomoService> logger;

        private readonly OganiMaterContext _dbContext;

        public readonly string accessKey = "F8BBA842ECF85";
        public readonly string secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";

        public MomoService(OganiMaterContext _dbContext)
        {
            this._dbContext = _dbContext;
            var logger = GlobalLogger.CreateLogger<MomoService>();
            this.logger = logger;
        }


        public async Task<MomoResponse?> CreatePayment(int userId, string domain)
        {
            List<Cart> cartItems = await this._dbContext.Carts.Include(c => c.Product).Where(c => c.UserId == userId).ToListAsync();

            if (cartItems.Count == 0)
            {
                throw new Exception("No items in cart to create an order.");
            }

            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey)) {
                throw new Exception("The accessKey or secretkey not available");
            }

            decimal totalAmount = cartItems.Sum(item => item.Quantity * (item.Product?.DiscountPrice ?? item.Price));

            Guid myuuid = Guid.NewGuid();
            string orderId = myuuid.ToString();

            var request = new CollectionLinkRequest
            {
                orderInfo = "Payment via MoMo",
                partnerCode = "MOMO",
                redirectUrl = $"{domain}/momo-redirect",
                ipnUrl = $"{domain}/momo-ipn",
                amount = (long)totalAmount,
                orderId = orderId,
                requestId = orderId,
                requestType = "payWithMethod",
                extraData = "",
                partnerName = "Your Store Name",
                storeId = "Your Store ID",
                autoCapture = true,
                lang = "vi",
            };

            var rawSignature = CreateRawSignature(request);
            request.signature = this.GetSignature(rawSignature);

            this.logger.LogInformation($"RawSignature CreatePayment: { rawSignature}");

            StringContent httpContent = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", httpContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to create MoMo payment. Status: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MomoResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public string GetSignature(string text)
        {
            
                ASCIIEncoding encoding = new ASCIIEncoding();

                byte[] textBytes = encoding.GetBytes(text);
                byte[] keyBytes = encoding.GetBytes(this.secretKey);

                byte[] hashBytes;

                using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                    hashBytes = hash.ComputeHash(textBytes);

                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }


        public string CreateRawSignature(CollectionLinkRequest request)
        {
           
            var rawSignature = "accessKey=" + this.accessKey + "&amount=" + request.amount + "&extraData=" + request.extraData + "&ipnUrl=" + request.ipnUrl + "&orderId=" + request.orderId + "&orderInfo=" + request.orderInfo + "&partnerCode=" + request.partnerCode + "&redirectUrl=" + request.redirectUrl + "&requestId=" + request.requestId + "&requestType=" + request.requestType;

            return rawSignature;
        }

        private async Task<bool> CheckTransactionStatusFromMomo(MomoConfirmDto momoConfirmDto)
        {
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/query";
            string rawSignature = $"accessKey={this.accessKey}&orderId={momoConfirmDto.orderId}&partnerCode={momoConfirmDto.partnerCode}&requestId={momoConfirmDto.requestId}";

            string signature = this.GetSignature(rawSignature);

            momoConfirmDto.signature = signature;

            using var httpClient = new HttpClient();
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(momoConfirmDto), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                MomoTransactionStatusResponse? resMomo = System.Text.Json.JsonSerializer.Deserialize<MomoTransactionStatusResponse>(jsonResponse);

                this.logger.LogInformation($"Momo response Code: {resMomo?.resultCode}");
                this.logger.LogInformation($"Momo response orderId: {resMomo?.orderId}");
                if (resMomo?.resultCode == 0 && resMomo.orderId == momoConfirmDto.orderId) return true;


                return false;
            }
            else
            {
                this.logger.LogError($"MoMo API call failed with status: {response.StatusCode}");
                return false;
            }

        }

        public async Task<ConfirmPaymentResultDto> ConfirmPaymentFromMomo(int userId, MomoConfirmDto momoConfirmDto, User user)
        {
            try
            {
                bool isSuccess = await this.CheckTransactionStatusFromMomo(momoConfirmDto);

                if (!isSuccess) return new ConfirmPaymentResultDto { IsSuccess = false, Amount = 0 };

                List<Cart> cartItems = await this._dbContext.Carts
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId).ToListAsync();

                if(cartItems.Count == 0) return new ConfirmPaymentResultDto { IsSuccess = false, Amount = 0 };

                decimal amount = 0;

                foreach (Cart item in cartItems)
                {
                    amount += item.Quantity * (item.Product?.DiscountPrice ?? item.Price);
                    Order newOrder = new Order
                    {
                        MEM_ID = user.UserId,
                        PROD_ID = item.PRO_ID,
                        Quantity = item.Quantity,
                        OrderDate = DateTime.UtcNow,
                        TotalPrice = item.Quantity * (item.Product?.DiscountPrice ?? item.Price),
                        Status = (int)OrderStatus.Confirmed,
                        IsPaid = true,
                        PaymentMethod = "MoMo",
                        Address = user.Address,
                        CustomerName = user.FirstName + " " + user.LastName,
                        Phone = item.User.Phone,
                        Discount = item.DiscountPrice,
                        CreatedBy = item.User.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = user.UpdatedBy,
                        UpdatedDate = DateTime.UtcNow
                    };

                    this._dbContext.Orders.Add(newOrder);
                }
                this._dbContext.Carts.RemoveRange(cartItems);
                await this._dbContext.SaveChangesAsync();

                return new ConfirmPaymentResultDto { Amount = amount, IsSuccess = true};
            }
            catch (Exception ex) {
                this.logger.LogError($"Error ConfirmPaymentFromMomo: {ex.Message}");
                this.logger.LogError($"Error ConfirmPaymentFromMomo track : {ex.StackTrace}");
                return new ConfirmPaymentResultDto { IsSuccess = false, Amount = 0 };
            }
        }

    }
}
