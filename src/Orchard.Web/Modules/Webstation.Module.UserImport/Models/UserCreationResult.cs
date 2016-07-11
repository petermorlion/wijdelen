using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security;
using Orchard.Localization;

namespace Webstation.Module.UserImport.Models
{
    public class UserCreationResult
    {
        //Constructors
        public UserCreationResult(UserCreationInput input)
        {
            Input = input;
            Valid = true;
        }
        //End Constructors

        //Properties
        public UserCreationInput Input { get; set; }
        public List<UserCreationResultMessage> Messages { get; set; }
        public bool Valid { get; set; }
        //End Properties

        //Functions
        public void AddMessage(LocalizedString message) { AddMessage(message.ToString()); }
        public void AddMessage(string message)
        {
            AddUserCreationResultMessage(new UserCreationResultMessage(UserCreationResultMessageType.Information, message));
        }

        public void AddError(LocalizedString error) { AddError(error.ToString()); }
        public void AddError(string error)
        {
            if (Valid)
                Valid = false;

            AddUserCreationResultMessage(new UserCreationResultMessage(UserCreationResultMessageType.Error, error));
        }

        private void AddUserCreationResultMessage(UserCreationResultMessage message)
        {
            if (Messages == null)
                Messages = new List<UserCreationResultMessage>();

            Messages.Add(message);
        }
        //End Functions
    }

    public enum UserCreationResultMessageType
    {
        Information,
        Warning,
        Error
    }

    public class UserCreationResultMessage
    {
        public UserCreationResultMessage(UserCreationResultMessageType type, string message)
        {
            Type = type;
            Message = message;
        }
        public UserCreationResultMessageType Type { get; set; }
        public string Message { get; set; }
    }
}