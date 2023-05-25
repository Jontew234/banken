using Platinum.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Platinum.Areas.Identity.Data
{
  
    public  class SortListHelperMethods<T> where T : IList
    {
        public T SortListByTimeAscending<TItem>(T item) where TItem : class
        {
            var sortedList = item.Cast<TItem>().OrderBy(x => (DateTime)x.GetType().GetProperty("Date").GetValue(x)).ToList();

            return (T)(object)sortedList;
            
        }

        public T SortListByTimeDescending<TItem>(T item) where TItem : class
        {

            var sortedList = item.Cast<TItem>().OrderByDescending(x => (DateTime)x.GetType().GetProperty("Date").GetValue(x)).ToList();

            return (T)(object)sortedList;
        }

        public T SortListByAmountAscending<TItem>(T item) where TItem : class
        {

            var sortedList = item.Cast<TItem>().OrderBy(x => (decimal)x.GetType().GetProperty("Amount").GetValue(x)).ToList();

            return (T)(object)sortedList;
        }

        public T SortListByAmountDescending<TItem>(T item) where TItem : class
        {

            var sortedList = item.Cast<TItem>().OrderByDescending(x => (decimal)x.GetType().GetProperty("Amount").GetValue(x)).ToList();

            return (T)(object)sortedList;
        }

        public T SortListByAccountNumberTo<TItem>(T item, string toBankAccount) where TItem : class
        {
            var sortedList = item.Cast<TItem>()
                            .Where(x => (string)x.GetType().GetProperty("ToBankAccount").GetValue(x) == toBankAccount)
                            .OrderByDescending(x => (string)x.GetType().GetProperty("ToBankAccount").GetValue(x))
                            .ToList();

            return (T)(object)sortedList;

        }


        public T SortListByAccountNumberFrom<TItem>(T item, string fromBankAccount) where TItem : class
        {
            var sortedList = item.Cast<TItem>()
                            .Where(x => (string)x.GetType().GetProperty("FromBankAccount").GetValue(x) == fromBankAccount)
                            .OrderByDescending(x => (string)x.GetType().GetProperty("FromBankAccount").GetValue(x))
                            .ToList();

            return (T)(object)sortedList;

        }

        
        public T SortListByDate<TItem>(T item, DateTime newDate) where TItem : class
        {
            var sortedList = item.Cast<TItem>()
                      .Where(x => ((DateTime)x.GetType().GetProperty("Date").GetValue(x)).Date == newDate.Date)
                      .ToList();


            return (T)(object)sortedList;
        }

        public T SortListByRiskAscending<TItem>(T item) where TItem : class
        {
            var sortedList = item.Cast<TItem>().OrderBy(x => int.Parse((x as dynamic).Risk.Split('/')[0])).ToList();

            return (T)(object)sortedList;
        }

        public T SortListByRiskDescending<TItem>(T item) where TItem : class
        {
            var sortedList = item.Cast<TItem>().OrderByDescending(x => int.Parse((x as dynamic).Risk.Split('/')[0])).ToList();

            return (T)(object)sortedList;
        }

        public T SortListByType<TItem>(T item , string search) where TItem : class
        {
            var sortedList = item.Cast<TItem>().Where(x => x.GetType().GetProperty("Type").GetValue(x).ToString()
            .ToLower().Equals(search.ToLower())).ToList();

            return (T)(object)sortedList;
        }

        public T SortListByExchange<TItem>(T item, string search) where TItem : class
        {
            var sortedList = item.Cast<TItem>().Where(x => x.GetType().GetProperty("Exchange").GetValue(x).ToString()
            .ToLower().Equals(search.ToLower())).ToList();

            return (T)(object)sortedList;
        }

        public T SortListByName<TItem>(T item, string search) where TItem : class
        {
            var sortedList = item.Cast<TItem>().Where(x => x.GetType().GetProperty("Name").GetValue(x).ToString()
            .ToLower().Equals(search.ToLower())).ToList();

            return (T)(object)sortedList;
        }

        public T SortListByAccounType<TItem>(T item, string search)
        {

            var sortedList = item.Cast<TItem>().Where(x => x.GetType().GetProperty("AccountType").GetValue(x).ToString()
            .ToLower().Equals(search.ToLower())).ToList();

            return (T)(object)sortedList;
        }
    }
}
