using Platinum.Controllers;

namespace Platinum.Areas.Identity.Data
{
    public class MyTimerHostedService : IHostedService , IDisposable
    {
        private readonly Events _myTaskService;
        private Timer _timer;
     

        public MyTimerHostedService(Events myTaskService)
        {
            _myTaskService = myTaskService;
          
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
              if(_timer is null) 
                {
                    _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(12));

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
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                await Task.Delay(TimeSpan.FromHours(12), cancellationToken);
                _timer.Change(TimeSpan.Zero, TimeSpan.FromHours(12));
             
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
            }
          
        }


        // eventhandler som kallar på en metod som i sin tur kallar på alla metoder som ska köras
        // hur ska denna triggas?
        private async void DoWork(object state)
        {
          
            await _myTaskService.FinanceEvents();
            
        }

        public void Dispose()
        {
            _timer?.Dispose();
          
        }


    }

}
