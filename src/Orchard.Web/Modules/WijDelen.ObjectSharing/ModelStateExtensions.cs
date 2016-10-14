using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using Orchard.Localization;

namespace WijDelen.ObjectSharing {
    public static class ModelStateExtensions {
        public static void AddModelError<TModel, TProperty>(this ModelStateDictionary modelState, Expression<Func<TModel, TProperty>> ex, LocalizedString message)
        {
            var key = ExpressionHelper.GetExpressionText(ex);
            modelState.AddModelError(key, message.ToString());
        }
    }
}