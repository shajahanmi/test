using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransEmirates.Applications.VerticsERPPlus.DataModel.Systems;

namespace TransEmirates.Library.HistoryManager
{
    public class Application
    {
        public string ConnectionString { get; set; } = "";
        public System.Data.SqlClient.SqlTransaction? Transaction { get; set; }
        public enum TransactionTypes
        {
            insert,
            update,
            delete
        }
        public async Task<bool> Register(string transactionName, int transactionId, TransactionTypes transactionType, string oldData, string newData, int userReference, System.DateTime transactionDate)
        {
            TransEmirates.Applications.VerticsERPPlus.DataAccessLayer.Systems.TransactionHistory th = new Applications.VerticsERPPlus.DataAccessLayer.Systems.TransactionHistory();
            th.SetConnectionString(ConnectionString);
            TransEmirates.Applications.VerticsERPPlus.DataModel.Systems.TransactionHistoryEntity te = new Applications.VerticsERPPlus.DataModel.Systems.TransactionHistoryEntity();
            te.history_id = 0;
            te.transaction_name = transactionName;
            te.transaction_id = transactionId;
            te.transaction_type = transactionType == TransactionTypes.insert ? "I" : transactionType == TransactionTypes.update ? "U" : "D";
            if (transactionType == TransactionTypes.insert)
            {
                te.old_data = "";
                te.new_data = newData;
            }
            else if (transactionType == TransactionTypes.update)
            {
                te.old_data = oldData;
                te.new_data = newData;
            }
            else
            {
                te.old_data = oldData;
                te.new_data = "";
            }
            te.user_reference = userReference;
            te.transaction_date = transactionDate;
            return await th.Save(te, Transaction!);
        }
        public async Task<List<HistorySummaryData>> GetHistory(string transactionName, int transactionId)
        {
            Applications.VerticsERPPlus.DataAccessLayer.Systems.TransactionHistory th = new Applications.VerticsERPPlus.DataAccessLayer.Systems.TransactionHistory();
            th.SetConnectionString(ConnectionString);
            th.RetrievalArguments.Add(new ApplicationFramework.RetrievalArgument("transaction_name", transactionName));
            th.RetrievalArguments.Add(new ApplicationFramework.RetrievalArgument("transaction_id", transactionId));

            _ = await th.Retrieve(Transaction!);

            Applications.VerticsERPPlus.DataAccessLayer.Systems.UserInfo users = new Applications.VerticsERPPlus.DataAccessLayer.Systems.UserInfo();
            users.SetConnectionString(ConnectionString);
            _ = await users.Retrieve(Transaction!);
            List<UserInfoEntity> userData = (List<UserInfoEntity>)users.Result;

            List<TransactionHistoryEntity> data = (List<TransactionHistoryEntity>)th.Result;
            var reqData = new List<HistorySummaryData>();
            foreach (var d in data)
            {
                HistorySummaryData hsd = new HistorySummaryData();
                hsd.transaction_type = d.transaction_type;
                hsd.transaction_date = d.transaction_date;
                hsd.user_reference = d.user_reference;
                var uinfo = userData.Where(item => item.user_reference == d.user_reference).Select(item => item.name).ToList();
                hsd.user_name = uinfo[0];
                reqData.Add(hsd);
            }
            return reqData;
        }
        public class HistorySummaryData
        {
            public int user_reference { get; set; } = 0;
            public string user_name { get; set; } = "";
            public string transaction_type { get; set; } = "";
            public System.DateTime transaction_date { get; set; }
        }
    }
}
