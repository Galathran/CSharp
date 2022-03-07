using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SzobeliVizsga2
{
    class Program
    {
        enum HallgatoAllapot { felkeszul, elkeszult, vizsgazik, hazament}
        static void Main(string[] args)
        {
            Random rnd = new Random();
            HallgatoAllapot ha = HallgatoAllapot.felkeszul;
            object hallgatoLock = new object();

            Task hallgatoSzal = new Task(() => {
                Console.WriteLine("A hallgató felkészül...");
                Thread.Sleep(rnd.Next(1500, 2001));
                Console.WriteLine("A hallgató kész!");
                ha = HallgatoAllapot.elkeszult;
                lock (hallgatoLock)
                    Monitor.Wait(hallgatoLock);
                ha = HallgatoAllapot.vizsgazik;
                Console.WriteLine("A hallgató vizsgázik!");
                ////ha fix 2 mp a vizsga
                //Thread.Sleep(2000);

                Thread.Sleep(rnd.Next(1000, 5001));
                lock (hallgatoLock)
                    Monitor.Pulse(hallgatoLock);

                ha = HallgatoAllapot.hazament;
                Console.WriteLine("A hallgató végzett, hazamegy");
            }, TaskCreationOptions.LongRunning);

            Task vizsgaztatoSzal = new Task(() => {
                Console.WriteLine("A vizsgáztató vár a hallgatóra...");
                //while (ha != HallgatoAllapot.elkeszult) ;
                //ez amugy busy waiting, de mikrosecenkent ujraertekeli a feltetelt
                while (ha != HallgatoAllapot.elkeszult)
                    Thread.Sleep(100);
                Console.WriteLine("A vizsgáztató szólítja a hallgatót!");
                lock (hallgatoLock)
                    Monitor.Pulse(hallgatoLock);
                Console.WriteLine("A vizsgáztató vizsgáztat.");
                ////ha fix 2 mp a vizsga
                //Thread.Sleep(2000);

                lock (hallgatoLock)
                    Monitor.Wait(hallgatoLock);

                Console.WriteLine("A vizsgáztató is végzett, hazamegy.");
            }, TaskCreationOptions.LongRunning);

            vizsgaztatoSzal.Start();
            hallgatoSzal.Start();

            Console.ReadLine();
        }
    }
}
