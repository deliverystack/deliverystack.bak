namespace csconapp
{
    using System;

    public class OutputWriter
    {
        public void Message(object sender, string message)
        {
            string source = typeof(Program).ToString();

            if (sender != null)
            {
                source = sender.GetType().ToString();
            }

            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fffffff")} : {sender} : {message}");
        }
    }
}
