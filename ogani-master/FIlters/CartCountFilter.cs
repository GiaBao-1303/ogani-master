using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ogani_master.Models;

public class CartCountFilter : ActionFilterAttribute
{
	public override void OnActionExecuting(ActionExecutingContext context)
	{



		var cartCount = context.HttpContext.Items["CartCount"];
		var cartTotal = context.HttpContext.Items["CartTotal"];
		if (context.Controller is Controller controller)
		{
			controller.ViewBag.CartCount = cartCount != null ? (int)cartCount : 0;
            controller.ViewBag.CartTotal = cartTotal != null ? (decimal)cartTotal : 0m;
        }

		base.OnActionExecuting(context);
	}
}
