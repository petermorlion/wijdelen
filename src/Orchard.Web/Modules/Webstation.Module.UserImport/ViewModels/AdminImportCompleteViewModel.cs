using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Webstation.Module.UserImport.Models;

namespace Webstation.Module.UserImport.ViewModels
{
    public class AdminImportCompleteViewModel
    {
        public IEnumerable<UserCreationResult> UserCreationResults;
    }
}