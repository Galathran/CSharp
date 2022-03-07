using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HallgatoVizsgaztato
{
    enum hallgatoStatus { felkeszul, elkeszult, vizsgazik, hazament }
    class Program
    {
        
        static void Main(string[] args)
        {
            #region minta
            //Random rnd = new Random();
            //object hallgatoLock = new object();
            //hallgatoStatus status = hallgatoStatus.felkeszul;

            //Task hallgato = new Task(() =>
            //{
            //    Console.WriteLine("Hallgató felkészül");
            //    Thread.Sleep(rnd.Next(750, 1250));
            //    Console.WriteLine("Hallgató felkészült");
            //    status = hallgatoStatus.elkeszult;
            //    lock (hallgatoLock)
            //        Monitor.Wait(hallgatoLock);
            //    //ide akkor lep, ha a vizsgaztato ebreszti
            //    status = hallgatoStatus.vizsgazik;
            //    Console.WriteLine("Hallgató vizsgázik");
            //}, TaskCreationOptions.LongRunning);

            //Task vizsgaztato = new Task(() =>
            //{
            //    Console.WriteLine("Vizsgáztató nézelődik a teremben");
            //    while (status != hallgatoStatus.elkeszult)
            //    {
            //        Thread.Sleep(rnd.Next(100, 300));
            //        //"busy waiting"
            //    }
            //    lock (hallgatoLock)
            //        Monitor.Pulse(hallgatoLock);
            //    Console.WriteLine("Vizsgáztató vizsgáztat");
            //}, TaskCreationOptions.LongRunning);

            //hallgato.Start(); vizsgaztato.Start();
            #endregion

            #region final
            List<Hallgato> hs = Enumerable.Range(0, 10).Select(x => new Hallgato()).ToList();
            List<Vizsgaztato> vs = Enumerable.Range(0, 3).Select(x => new Vizsgaztato()).ToList();

            hs.Select(x => new Task(() => x.Letezik(), TaskCreationOptions.LongRunning))
                .ToList()
                .ForEach(x => x.Start());
            vs.Select(x => new Task(() => x.Letezik(hs), TaskCreationOptions.LongRunning))
                .ToList()
                .ForEach(x => x.Start());

            new Task(() =>
            {
                while (hs.Any(x => x.Status != hallgatoStatus.hazament))
                {
                    Console.Clear();
                    foreach (var h in hs)
                        Console.WriteLine($"Hallgató #{h.ID}: {h.Status}");
                    foreach (var v in vs)
                        Console.WriteLine($"Vizsgáztató #{v.ID}: {(v.Vizsgaztatott == null ? "senki" : v.Vizsgaztatott.ID.ToString())}");
                    Thread.Sleep(40);
                }
                Console.Clear();
                foreach (var h in hs)
                    Console.WriteLine($"Hallgató #{h.ID}: {h.Status}");
                foreach (var v in vs)
                    Console.WriteLine($"Vizsgáztató #{v.ID}: {(v.Vizsgaztatott == null ? "senki" : v.Vizsgaztatott.ID.ToString())}");
                Console.WriteLine("VÉGE");
            }, TaskCreationOptions.LongRunning)
                .Start();
            #endregion

            Console.ReadLine();
        }
    }

    class Hallgato
    {
        public object lockObject = new object();
        //ezen keresztul megy a jelzes
        static int numbering = 1;
        static Random rnd = new Random();
        public int ID { get; private set; }
        public hallgatoStatus Status { get; set; }
        //nem private set, a vizsgaztato allitja at!
        public Hallgato()
        {
            ID = numbering++;
            Status = hallgatoStatus.felkeszul;
            //letrejottekor felkeszul, nem varakozik a terem elott...
        }

        public void Letezik()
        {
            Thread.Sleep(rnd.Next(750, 3500));
            Status = hallgatoStatus.elkeszult;
            lock (lockObject)
                Monitor.Wait(lockObject);
            Thread.Sleep(1000);
            Status = hallgatoStatus.hazament;
        }
    }

    class Vizsgaztato
    {
        static object valasztasLock = new object();
        static int numbering = 1;
        public int ID { get; private set; }
        public Hallgato Vizsgaztatott { get; private set; }
        public Vizsgaztato()
        {
            ID = numbering++;
            Vizsgaztatott = null;   //by default
        }

        public void Letezik(List<Hallgato> hs)
        {
            while (hs.Any(x => x.Status != hallgatoStatus.hazament))
            {
                lock (valasztasLock)
                {
                    var keszenvannak = hs.Where(x => x.Status == hallgatoStatus.elkeszult);
                    if (keszenvannak.Count() > 0)
                    {
                        Vizsgaztatott = keszenvannak.First();
                        Vizsgaztatott.Status = hallgatoStatus.vizsgazik;
                    }
                }
                if (Vizsgaztatott != null)
                {
                    lock (Vizsgaztatott.lockObject)
                        Monitor.Pulse(Vizsgaztatott.lockObject);
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
