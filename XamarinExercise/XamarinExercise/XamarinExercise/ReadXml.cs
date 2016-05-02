using System.Linq;
using System.Xml.Linq;

namespace XamarinExercise
{
    /*
     * @author Venkatakrishna Vanparthi
     * This class was used to read the Xml from given URL.
     * 
     */

    public class ReadXml
    {
        #region Maintaining Singleton Instance

        private static ReadXml _instance;
        private static readonly object Padlock = new object();

        ReadXml()
        {
        }

        public static ReadXml Instance
        {
            get
            {
                lock (Padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new ReadXml();
                    }
                    return _instance;
                }
            }
        }



        #endregion

        /*
         * 
         * This method was used to read image url from server Xml which will be used for displaying squareview.
         * 
         */

        public RandomData ReadImageUrl()
        {
            var doc = XDocument.Load("http://www.colourlovers.com/api/patterns/random");

            var data = from item in doc.Descendants("pattern")
                let xElement = item.Element("imageUrl")
                where xElement != null
                select new
                {
                    Name = xElement.Value
                };


            var title = from item in doc.Descendants("pattern")
                let xElement = item.Element("title")
                where xElement != null
                select new
                {
                    Name = xElement.Value
                };

            var randomData = new RandomData
            {
                Data = data.Select(item => item.Name).FirstOrDefault(),
                Title = title.Select(item => item.Name).FirstOrDefault()
            };

            return randomData;
        }

        /*
         * 
         * This method was used to read the Hex color code from server Xml which will be used for circle view.
         * 
         */

        public RandomData ReadHexColor()
        {
            var doc = XDocument.Load("http://www.colourlovers.com/api/colors/random");

            var data = from item in doc.Descendants("color")
                let xElement = item.Element("hex")
                where xElement != null
                select new
                {
                    Name = xElement.Value
                };

            var title = from item in doc.Descendants("color")
                let xElement = item.Element("title")
                where xElement != null
                select new
                {
                    Name = xElement.Value
                };

            var randomData = new RandomData
            {
                Data = "#" + data.Select(item => item.Name).FirstOrDefault(),
                Title = title.Select(item => item.Name).FirstOrDefault()
            };

            return randomData;

        }

    }
}
