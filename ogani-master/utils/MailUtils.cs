using ogani_master.dto;
using System.Net.Mail;
using System.Net;
using System.Text;
using ogani_master.enums;

namespace ogani_master.utils
{
    public static class MailUtils
    {
        public static async Task<bool> SendMailAsync(string from, string to, string subject, string body, SmtpClient client, List<MessageMailDto> messageMailDtos)
        {
            using (var message = CreateMailMessage(from, to, subject, body, messageMailDtos))
            {
                try
                {
                    await client.SendMailAsync(message);
                    return true;
                }
                catch (Exception ex)
                {
                    LogError($"Failed to send email: {ex.Message}");
                    return false;
                }
            }
        }

        public static async Task<bool> SendMailGoogleSmtpOrderStatusAsync(string to, string subject, OrderStatus status, string customerName, string companyName, string trackingLink)
        {
            string? email = Environment.GetEnvironmentVariable("_gmailsend");
            string? pwd = Environment.GetEnvironmentVariable("PWD_EMAIL");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
                throw new Exception("Email credentials not available in environment variables.");

            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(email, pwd);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.Timeout = 20000;

                string body = GetEmailTemplateBasedOnStatus(status, customerName, companyName, trackingLink);
                return await SendMailOrderStatusAsync(email, to, subject, body, client);
            }
        }

		public static async Task<bool> SendMailGoogleSmtpForgotPasswordAsync(
	        string to,
	        string subject,
	        string customerName,
	        string secretUrl,
            string? ttl = "3 phút"
        )
		{
			string? email = Environment.GetEnvironmentVariable("_gmailsend");
			string? pwd = Environment.GetEnvironmentVariable("PWD_EMAIL");

			if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
				throw new Exception("Email credentials not available in environment variables.");

			using (var client = new SmtpClient("smtp.gmail.com"))
			{
				client.Port = 587;
				client.Credentials = new NetworkCredential(email, pwd);
				client.DeliveryMethod = SmtpDeliveryMethod.Network;
				client.EnableSsl = true;
				client.Timeout = 20000;

				string body = TemplateForgotPassword(customerName, secretUrl, ttl);
				return await SendMailForgotPasswordAsync(email, to, subject, body, client);
			}
		}

        public static async Task<bool> SendMailGoogleSmtpConfirmEmailAsync(
            string to,
            string subject,
            string customerName,
            string secretOtp,
            string? ttl = "3 phút"
        )
        {
            string? email = Environment.GetEnvironmentVariable("_gmailsend");
            string? pwd = Environment.GetEnvironmentVariable("PWD_EMAIL");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
                throw new Exception("Email credentials not available in environment variables.");

            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(email, pwd);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.Timeout = 20000;

                string body = TemplateConfirmEmail(customerName, secretOtp, ttl);
                return await SendMailConfirmEmailAsync(email, to, subject, body, client);
            }
        }

        private static async Task<bool> SendMailConfirmEmailAsync(
           string from,
           string to,
           string subject,
           string body,
           SmtpClient client)
        {
            try
            {
                using (var message = new MailMessage(from, to))
                {
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    await client.SendMailAsync(message);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async Task<bool> SendMailForgotPasswordAsync(
			string from,
			string to,
			string subject,
			string body,
			SmtpClient client)
		{
			try
			{
				using (var message = new MailMessage(from, to))
				{
					message.Subject = subject;
					message.Body = body;
					message.IsBodyHtml = true;
					await client.SendMailAsync(message);
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static async Task<bool> SendMailOrderStatusAsync(string from, string to, string subject, string body, SmtpClient client)
        {
            using (var message = CreateMailMessageOrderStatus(from, to, subject, body))
            {
                try
                {
                    await client.SendMailAsync(message);
                    return true;
                }
                catch (Exception ex)
                {
                    LogError($"Failed to send email: {ex.Message}");
                    return false;
                }
            }
        }

        private static MailMessage CreateMailMessageOrderStatus(string from, string to, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = true,
            };
            message.To.Add(to);
            message.ReplyToList.Add(new MailAddress(from));
            message.Sender = new MailAddress(from);
            return message;
        }


        public static async Task<bool> SendMailGoogleSmtpAsync(string to, string subject, string body, List<MessageMailDto> messageMailDtos)
        {
            string? email = Environment.GetEnvironmentVariable("_gmailsend");
            string? pwd = Environment.GetEnvironmentVariable("PWD_EMAIL");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
                throw new Exception("Email credentials not available in environment variables.");

            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(email, pwd);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.Timeout = 20000;

                return await SendMailAsync(email, to, subject, body, client, messageMailDtos);
            }
        }

        private static string TemplateMailSendMessage(string customerName, string companyName, List<MessageMailDto> productDetails, string trackingLink)
        {
            string productDetailHtml = string.Join(string.Empty, productDetails.Select(product => $@"
                <tr>
                    <td style=""border: 1px solid #dddddd; padding: 8px;"">{product.prodName}</td>
                    <td style=""border: 1px solid #dddddd; padding: 8px;"">{product.quantity}</td>
                    <td style=""border: 1px solid #dddddd; padding: 8px;"">${product.price:#,##0}</td>
                    <td style=""border: 1px solid #dddddd; padding: 8px;"">${(product.quantity * product.price):#,##0}</td>
                </tr>"));

            int totalOrderPrice = productDetails.Sum(product => product.quantity * product.price);

            string template = @"
            <table style=""width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden; font-family: Arial, sans-serif; color: #333;"">
                <tr>
                    <td style=""background-color: #28a745; color: #ffffff; text-align: center; padding: 20px;"">
                        <h1 style=""margin: 0; font-size: 24px;"">Cảm Ơn Bạn Đã Đặt Hàng!</h1>
                    </td>
                </tr>
                <tr>
                    <td style=""padding: 20px; line-height: 1.6;"">
                        <p style=""margin: 0 0 10px;"">Xin chào <strong>{0}</strong>,</p>
                        <p style=""margin: 0 0 10px;"">Cảm ơn bạn đã tin tưởng và đặt hàng tại <strong>{1}</strong>. Đơn hàng của bạn đã được chúng tôi nhận và đang trong quá trình xử lý.</p>
                        <p style=""margin: 0 0 10px;"">Dưới đây là chi tiết đơn hàng của bạn:</p>
                        <table style=""width: 100%; border-collapse: collapse; margin: 10px 0;"">
                            <thead>
                                <tr>
                                    <th style=""border: 1px solid #dddddd; padding: 8px; background-color: #f2f2f2;"">Sản Phẩm</th>
                                    <th style=""border: 1px solid #dddddd; padding: 8px; background-color: #f2f2f2;"">Số Lượng</th>
                                    <th style=""border: 1px solid #dddddd; padding: 8px; background-color: #f2f2f2;"">Giá</th>
                                    <th style=""border: 1px solid #dddddd; padding: 8px; background-color: #f2f2f2;"">Tổng Giá</th>
                                </tr>
                            </thead>
                            <tbody>
                                {2}
                                <tr>
                                    <td colspan=""3"" style=""border: 1px solid #dddddd; padding: 8px; text-align: right;""><strong>Tổng Cộng:</strong></td>
                                    <td style=""border: 1px solid #dddddd; padding: 8px;""><strong>{3:#,##0} VNĐ</strong></td>
                                </tr>
                            </tbody>
                        </table>
                        <p style=""margin: 0 0 10px;"">Bạn có thể theo dõi trạng thái đơn hàng của mình bằng cách nhấn vào nút dưới đây:</p>
                        <div style=""text-align: center; margin: 20px 0;"">
                            <a href=""{4}"" style=""display: inline-block; background-color: #28a745; color: #ffffff; text-decoration: none; padding: 10px 20px; border-radius: 5px; font-size: 16px;"">Theo Dõi Đơn Hàng</a>
                        </div>
                        <p style=""margin: 0;"">Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
                        <p style=""margin: 20px 0 0;"">Trân trọng,<br>Đội Ngũ <strong>{1}</strong></p>
                    </td>
                </tr>
                <tr>
                    <td style=""text-align: center; padding: 10px; background-color: #f9f9f9; color: #777; font-size: 12px;"">
                        <p style=""margin: 0;"">&copy; 2024 {1}. Tất cả các quyền được bảo lưu.</p>
                        <p style=""margin: 0;"">Bạn nhận được email này vì đã đặt hàng trên nền tảng của chúng tôi.</p>
                    </td>
                </tr>
            </table>";

            return string.Format(template, customerName, companyName, productDetailHtml, totalOrderPrice, trackingLink);
        }

        private static MailMessage CreateMailMessage(string from, string to, string subject, string body, List<MessageMailDto> messageMailDtos)
        {
            var message = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = TemplateMailSendMessage(messageMailDtos.First().customerName, messageMailDtos.First().companyName, messageMailDtos, "Ogani-master"),
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = true,
            };
            message.To.Add(to);
            message.ReplyToList.Add(new MailAddress(from));
            message.Sender = new MailAddress(from);
            return message;
        }



        private static string GetEmailTemplateBasedOnStatus(OrderStatus status, string customerName, string companyName, string trackingLink)
        {
            return status switch
            {
                OrderStatus.Shipping => TemplateOutForDelivery(customerName, companyName, trackingLink),
                OrderStatus.Delivered => TemplateDeliveredSuccessfully(customerName, companyName),
                OrderStatus.Canceled => TemplateOrderCanceled(customerName, companyName),
                OrderStatus.Returned => TemplateOrderReturned(customerName, companyName),
            };
        }

        public static async Task<bool> SendMailGoogleSmtpPaymentSuccessAsync(
            string to,
            string subject,
            string customerName,
            string companyName,
            string orderNumber,
            string paymentMethod,
            string amount,
            DateTime paymentTime,
            string currency = "VNĐ"
        )
        {
            string? email = Environment.GetEnvironmentVariable("_gmailsend");
            string? pwd = Environment.GetEnvironmentVariable("PWD_EMAIL");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
                throw new Exception("Email credentials not available in environment variables.");

            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(email, pwd);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.Timeout = 20000;

                string body = TemplatePaymentSuccess(customerName, companyName, orderNumber, paymentMethod, amount, paymentTime, currency);
                return await SendMailPaymentSuccessAsync(email, to, subject, body, client);
            }
        }

		public static string TemplateForgotPassword(string customerName, string resetUrl, string ttl)
		{
			return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Reset Password</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
        <div style='text-align: center; padding: 20px;'>
            <h1 style='color: #333333; margin-bottom: 20px;'>Yêu Cầu Đặt Lại Mật Khẩu</h1>
        </div>
        
        <div style='padding: 20px; color: #666666; font-size: 16px; line-height: 1.5;'>
            <p>Xin chào {customerName},</p>
            
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
            
            <p>Để đặt lại mật khẩu, vui lòng nhấp vào nút bên dưới:</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{resetUrl}' style='background-color: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Đặt Lại Mật Khẩu</a>
            </div>
            
            <p>Hoặc copy đường link sau vào trình duyệt:</p>
            <p style='background-color: #f8f8f8; padding: 10px; border-radius: 3px; word-break: break-all;'>{resetUrl}</p>
            
            <p>Lưu ý: Link này chỉ có hiệu lực trong vòng {ttl}.</p>
            
            <p>Nếu bạn gặp bất kỳ vấn đề gì, vui lòng liên hệ với chúng tôi để được hỗ trợ.</p>
            
            <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #eeeeee;'>
                <p style='color: #999999; font-size: 14px;'>
                    Trân trọng,<br>
                    Team Support
                </p>
            </div>
        </div>
    </div>
</body>
</html>";
		}

        public static string TemplateConfirmEmail(string customerName, string otpCode, string ttl)
        {
            return $@"
<div style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
        <div style='text-align: center; padding: 20px;'>
            <h1 style='color: #333333; margin-bottom: 20px;'>Xác Thực Email</h1>
        </div>
        
        <div style='padding: 20px; color: #666666; font-size: 16px; line-height: 1.5;'>
            <p>Xin chào {customerName},</p>
            
            <p>Chúng tôi nhận được yêu cầu xác thực email cho tài khoản của bạn. Dưới đây là mã OTP của bạn:</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <p style='background-color: #f8f8f8; padding: 15px; border-radius: 5px; display: inline-block; font-size: 24px; font-weight: bold; color: #333333;'>{otpCode}</p>
            </div>
            
            <p>Vui lòng nhập mã này trong vòng {ttl} để hoàn tất xác thực.</p>
            
            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này. Nếu bạn gặp bất kỳ vấn đề gì, vui lòng liên hệ với chúng tôi để được hỗ trợ.</p>
            
            <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #eeeeee;'>
                <p style='color: #999999; font-size: 14px;'>
                    Trân trọng,<br>
                    Team Support
                </p>
            </div>
        </div>
    </div>
    </div>
";
           
        }

        private static async Task<bool> SendMailPaymentSuccessAsync(string from, string to, string subject, string body, SmtpClient client)
        {
            using (var message = CreateMailMessagePaymentSuccess(from, to, subject, body))
            {
                try
                {
                    await client.SendMailAsync(message);
                    return true;
                }
                catch (Exception ex)
                {
                    LogError($"Failed to send payment success email: {ex.Message}");
                    return false;
                }
            }
        }

        private static MailMessage CreateMailMessagePaymentSuccess(string from, string to, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = true,
            };
            message.To.Add(to);
            message.ReplyToList.Add(new MailAddress(from));
            message.Sender = new MailAddress(from);
            return message;
        }

        private static string TemplateOutForDelivery(string customerName, string companyName, string trackingLink)
        {
            return $@"
    <table style=""width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden; font-family: Arial, sans-serif; color: #333;"">
        <tr>
            <td style=""background-color: #17a2b8; color: #ffffff; text-align: center; padding: 20px;"">
                <h1 style=""margin: 0; font-size: 24px;"">Đang Giao Hàng</h1>
            </td>
        </tr>
        <tr>
            <td style=""padding: 20px; line-height: 1.6;"">
                <p>Xin chào <strong>{customerName}</strong>,</p>
                <p>Đơn hàng của bạn đang trên đường giao đến bạn.</p>
                <p>Bạn có thể theo dõi trạng thái đơn hàng của mình bằng cách nhấn vào nút dưới đây:</p>
                <div style=""text-align: center; margin: 20px 0;"">
                    <a href=""{trackingLink}"" style=""display: inline-block; background-color: #28a745; color: #ffffff; text-decoration: none; padding: 10px 20px; border-radius: 5px; font-size: 16px;"">Theo Dõi Đơn Hàng</a>
                </div>
                <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
                <p>Trân trọng,<br>Đội Ngũ <strong>{companyName}</strong></p>
            </td>
        </tr>
        <tr>
            <td style=""text-align: center; padding: 10px; background-color: #f9f9f9; color: #777; font-size: 12px;"">
                <p>&copy; 2024 {companyName}. Tất cả các quyền được bảo lưu.</p>
                <p>Bạn nhận được email này vì đã đặt hàng trên nền tảng của chúng tôi.</p>
            </td>
        </tr>
    </table>";
        }

        private static string TemplateDeliveredSuccessfully(string customerName, string companyName)
        {
            return $@"
    <table style=""width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden; font-family: Arial, sans-serif; color: #333;"">
        <tr>
            <td style=""background-color: #28a745; color: #ffffff; text-align: center; padding: 20px;"">
                <h1 style=""margin: 0; font-size: 24px;"">Đã Giao Hàng Thành Công</h1>
            </td>
        </tr>
        <tr>
            <td style=""padding: 20px; line-height: 1.6;"">
                <p>Xin chào <strong>{customerName}</strong>,</p>
                <p>Đơn hàng của bạn đã được giao thành công.</p>
                <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
                <p>Trân trọng,<br>Đội Ngũ <strong>{companyName}</strong></p>
            </td>
        </tr>
        <tr>
            <td style=""text-align: center; padding: 10px; background-color: #f9f9f9; color: #777; font-size: 12px;"">
                <p>&copy; 2024 {companyName}. Tất cả các quyền được bảo lưu.</p>
                <p>Bạn nhận được email này vì đã đặt hàng trên nền tảng của chúng tôi.</p>
            </td>
        </tr>
    </table>";
        }

        private static string TemplateOrderCanceled(string customerName, string companyName)
        {
            return $@"
    <table style=""width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden; font-family: Arial, sans-serif; color: #333;"">
        <tr>
            <td style=""background-color: #dc3545; color: #ffffff; text-align: center; padding: 20px;"">
                <h1 style=""margin: 0; font-size: 24px;"">Đơn Hàng Bị Hủy</h1>
            </td>
        </tr>
        <tr>
            <td style=""padding: 20px; line-height: 1.6;"">
                <p>Xin chào <strong>{customerName}</strong>,</p>
                <p>Rất tiếc, đơn hàng của bạn đã bị hủy.</p>
                <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
                <p>Trân trọng,<br>Đội Ngũ <strong>{companyName}</strong></p>
            </td>
        </tr>
        <tr>
            <td style=""text-align: center; padding: 10px; background-color: #f9f9f9; color: #777; font-size: 12px;"">
                <p>&copy; 2024 {companyName}. Tất cả các quyền được bảo lưu.</p>
                <p>Bạn nhận được email này vì đã đặt hàng trên nền tảng của chúng tôi.</p>
            </td>
        </tr>
    </table>";
        }

        private static string TemplateOrderReturned(string customerName, string companyName)
        {
            return $@"
    <table style=""width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden; font-family: Arial, sans-serif; color: #333;"">
        <tr>
            <td style=""background-color: #6c757d; color: #ffffff; text-align: center; padding: 20px;"">
                <h1 style=""margin: 0; font-size: 24px;"">Đơn Hàng Được Trả Lại</h1>
            </td>
        </tr>
        <tr>
            <td style=""padding: 20px; line-height: 1.6;"">
                <p>Xin chào <strong>{customerName}</strong>,</p>
                <p>Đơn hàng của bạn đã được trả lại.</p>
                <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
                <p>Trân trọng,<br>Đội Ngũ <strong>{companyName}</strong></p>
            </td>
        </tr>
        <tr>
            <td style=""text-align: center; padding: 10px; background-color: #f9f9f9; color: #777; font-size: 12px;"">
                <p>&copy; 2024 {companyName}. Tất cả các quyền được bảo lưu.</p>
                <p>Bạn nhận được email này vì đã đặt hàng trên nền tảng của chúng tôi.</p>
            </td>
        </tr>
    </table>";
        }

        private static string TemplatePaymentSuccess(string customerName, string companyName, string orderNumber, string paymentMethod, string amount, DateTime paymentTime, string currency = "VNĐ")
        {
            return $@"
    <table style=""width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden; font-family: Arial, sans-serif; color: #333;"">
        <tr>
            <td style=""background-color: #00c851; color: #ffffff; text-align: center; padding: 20px;"">
                <h1 style=""margin: 0; font-size: 24px;"">🎉 Thanh Toán Thành Công</h1>
            </td>
        </tr>
        <tr>
            <td style=""padding: 20px; line-height: 1.6;"">
                <p>Xin chào <strong>{customerName}</strong>,</p>
                <p>Chúng tôi xin thông báo rằng thanh toán cho đơn hàng của bạn đã được xử lý thành công.</p>
                
                <div style=""background-color: #f8f9fa; border-radius: 8px; padding: 15px; margin: 20px 0;"">
                    <h2 style=""color: #28a745; margin-top: 0; font-size: 18px;"">Chi Tiết Thanh Toán</h2>
                    <table style=""width: 100%; border-collapse: collapse;"">
                        <tr>
                            <td style=""padding: 8px 0; border-bottom: 1px solid #dee2e6;"">Mã đơn hàng:</td>
                            <td style=""padding: 8px 0; border-bottom: 1px solid #dee2e6; text-align: right;""><strong>#{orderNumber}</strong></td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; border-bottom: 1px solid #dee2e6;"">Phương thức thanh toán:</td>
                            <td style=""padding: 8px 0; border-bottom: 1px solid #dee2e6; text-align: right;""><strong>{paymentMethod}</strong></td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; border-bottom: 1px solid #dee2e6;"">Thời gian thanh toán:</td>
                            <td style=""padding: 8px 0; border-bottom: 1px solid #dee2e6; text-align: right;""><strong>{paymentTime:dd/MM/yyyy HH:mm}</strong></td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; font-size: 18px;"">Số tiền:</td>
                            <td style=""padding: 8px 0; text-align: right; font-size: 18px; color: #28a745;""><strong>{amount.Replace("$", "")} {currency}</strong></td>
                        </tr>
                    </table>
                </div>

                <div style=""text-align: center; margin: 30px 0; padding: 20px; background-color: #e8f5e9; border-radius: 8px;"">
                    <img src=""https://cdn-icons-png.flaticon.com/512/148/148767.png"" alt=""Success"" style=""width: 60px; height: 60px; margin-bottom: 10px;"">
                    <p style=""margin: 0; color: #2e7d32; font-size: 16px;"">Giao dịch của bạn đã được xử lý an toàn và bảo mật</p>
                </div>

                <p>Bạn có thể theo dõi trạng thái đơn hàng của mình trong tài khoản cá nhân trên website của chúng tôi.</p>
                <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
                <p style=""margin: 20px 0 0;"">Trân trọng,<br>Đội Ngũ <strong>{companyName}</strong></p>
            </td>
        </tr>
        <tr>
            <td style=""text-align: center; padding: 15px; background-color: #f9f9f9; color: #777; font-size: 12px;"">
                <p style=""margin: 0;"">&copy; 2024 {companyName}. Tất cả các quyền được bảo lưu.</p>
                <p style=""margin: 5px 0 0;"">Email này được gửi tự động. Vui lòng không trả lời email này.</p>
            </td>
        </tr>
    </table>";
        }

        private static void LogError(string message)
        {
            try
            {
                string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                string logFilePath = Path.Combine(logDirectory, "error_log.txt");
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log error: {ex.Message}");
            }
        }
    }
}
