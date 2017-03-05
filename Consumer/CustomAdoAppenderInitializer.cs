namespace Consumer
{
    using Consumer.Functions;

    public static class CustomAdoNetAppenderInitializer
    {
        public static void IntializeType()
        {
            //not used, but intialized to ensure this type can be loaded from config
            var customAdoNetAppender = new CustomAdoNetAppender();
        }
    }
}