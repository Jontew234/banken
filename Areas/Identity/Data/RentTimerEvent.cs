using System.Diagnostics;

namespace Platinum.Areas.Identity.Data
{
    public class RentTimerEvent : IHostedService
    {
        private readonly Events _myTaskService;
        private Timer timer;


        public RentTimerEvent(Events myTaskService)
        {
            _myTaskService = myTaskService;

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (timer is null)
                {

                    int daysUntilNext25th =  await SetTimeSpan();


                    timer = new Timer(DoWork, null, TimeSpan.FromDays(daysUntilNext25th), TimeSpan.FromHours(10));

                    // nu går den igång den 25 första månaden 
                    // sista i dowork bör vara att den kallar på en metod som justerar timern
                    // när den ska gå igång nästa intervall
                    //timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(10));

                }


            }
            catch (Exception ex)
            {
                CancellationToken cancellation = new CancellationToken();
                await StopAsync(cancellation);

            }

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                timer?.Change(Timeout.Infinite, Timeout.Infinite);
                await Task.Delay(TimeSpan.FromHours(12), cancellationToken);
                timer?.Change(TimeSpan.Zero, TimeSpan.FromHours(12));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
            }

        }

        // en metod för att uppdatera spannet egentligen samma kod
        // som startmetoden så bryt ut
        private async Task<int> SetTimeSpan()
        {
            var currentDate = DateTime.Today;

            var daysUntilNext25th = 25 - currentDate.Day;

            if (daysUntilNext25th < 0)
            {
                var daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                daysUntilNext25th = daysInMonth - currentDate.Day + 25;
            }
            return daysUntilNext25th;
        }
   
        private async void DoWork(object state)
        {

            try {
                // kalla på metoden som ska dra alla räntor på lånet
                await _myTaskService.PayRentEvent();
                
                int daysUntilNext25th = await SetTimeSpan();
               int next25InMilliSeconds = daysUntilNext25th * 86400000;
                 timer.Change(next25InMilliSeconds, next25InMilliSeconds);
               
           

            }
            catch (Exception e) 
            {
                
            }
            

        }
    }
}
