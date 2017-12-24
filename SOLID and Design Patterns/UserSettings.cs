using System;

namespace SOLID_and_Design_Patterns
{
    public class UserSettings
    {
        public Guid UserId { get; set; }  
        public string Alias { get; set; }  
        public bool SendNewsletter { get; set; }  
    }
}