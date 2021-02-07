using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace Currency_Rate_Tracking
{
    class calculateExchangeRate
    {
        double totalAmount = 0, totalGram = 0;
        XDocument tcmbdoviz = XDocument.Load("http://www.tcmb.gov.tr/kurlar/today.xml");
        Timer timer = new Timer();
        public void Start()
        {
            WriteToFile("1");
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 600000;
            timer.Enabled = true;
        }
        public void Stop()
        {
            WriteToFile("Service Durduruldu " + DateTime.Now);
        }
        float StringToFloat(string word)
        {
            float multiplier = 1;
            float result = 0;
            int pointerIndex = -1;

            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == '.' || word[i] == ',')
                {
                    pointerIndex = i;
                }
            }

            if (pointerIndex == -1)
                pointerIndex = word.Length;

            for (int k = pointerIndex - 1; k >= 0; k--)
            {
                int temp = word[k] - 48;
                result += multiplier * temp;
                multiplier *= 10;
            }
            float dividing = 10;

            for (int j = pointerIndex + 1; j < word.Length; j++)
            {
                int temp = word[j] - 48;
                result += temp / dividing;
                dividing *= 10;
            }

            return result;
        }
        public void WriteToFile(string Message)
        {
            ExchangeRate();

            var total = string.Format("{0:C} ₺",
                    totalAmount);
            total = total.Substring(1);
            var _total = total.Split(',');

            try
            {
                string file = @"C:\Users\Celilhan\Desktop\Programlar\Kur_Ozet.csv";
                string[] lines = new string[5000];
                int counter = 0;
                FileStream fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                //dosyadan satır satır okuyup textBox içine yazıdırıyoruz
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        lines[counter] = line;
                        counter++;
                        if (line == null) break;
                    }
                    reader.Close();
                }
                fileStream.Close();



                FileStream stream = File.Create(file);
                StreamWriter sw = new StreamWriter(stream);
                for (int i = 0; i < counter; i++)
                {
                    if (i == counter - 1)
                        sw.Write(lines[i]);
                    else
                        sw.Write(lines[i] + "\n");
                }

                sw.Write(DateTime.Now.ToString() + "," + _total[0]);
                sw.Close();
            }
            catch (Exception)
            {

            }
        }

        private void ExchangeRate()
        {
            totalAmount = 0;
            totalGram = 0;

            DollarEuroRates("EURO", 200);
            DollarEuroRates("ABD DOLARI", 0);

            GoldRates("https://altin.in/fiyat/cumhuriyet-altini", 5);
            GoldRates("https://altin.in/fiyat/yarim-altin", 1);
            GoldRates("https://altin.in/fiyat/gram-altin", 273.39);

        }

        private void DollarEuroRates(string value, double piece)
        {
            var kurbilgileri = from kurlar in tcmbdoviz.Descendants("Currency")
                               where kurlar.Element("Isim").Value == value
                               select new
                               {
                                   kuradiing = kurlar.Element("CurrencyName").Value,
                                   kuralis = kurlar.Element("ForexBuying").Value,
                                   kursatis = kurlar.Element("ForexSelling").Value
                               };
            foreach (var veriler in kurbilgileri)
            {
                totalAmount += StringToFloat(veriler.kuralis) * piece;
            }
        }

        private void GoldRates(string URL, double piece)
        {
            var url = new Uri(URL); // url oluştruduk
            var client = new WebClient(); // siteye erişim için client tanımladık
            var html = client.DownloadString(url); //sitenin html lini indirdik
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument(); //burada HtmlAgilityPack Kütüphanesini kullandık
            doc.LoadHtml(html); // indirdiğimiz sitenin html lini oluşturduğumuz dokumana dolduruyoruz
            var veri = doc.DocumentNode.SelectNodes("//*[@id='icerik']/div[1]/div[2]/div[4]/ul/li[2]")[0]; // siteden aldığımız xpath i buraya yazıp kaynak kısmını seçiyoruz
            var satis = doc.DocumentNode.SelectNodes("//*[@id='icerik']/div[1]/div[2]/div[4]/ul/li[3]")[0];
            if (veri != null)
            {
                totalAmount += StringToFloat(veri.InnerHtml) * piece;
            }
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service Message !:    " + DateTime.Now);
        }
    }
}