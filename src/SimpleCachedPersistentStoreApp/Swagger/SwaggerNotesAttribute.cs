using System;

namespace SimpleCachedPersistentStoreApp.Swagger
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerNotesAttribute : Attribute
    {
        public string Description { get; private set; }

        public SwaggerNotesAttribute(string description)
        {
            this.Description = description;
        }
    }
}