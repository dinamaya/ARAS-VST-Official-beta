using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Models.Complex;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class TestService(IConfigService configService) : ITestService
	{
		private string[] firstNames = { "James", "Emma", "Michael", "Olivia", "William", "Sophia", "Daniel", "Isabella", "Matthew", "Ava" };
		private string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Garcia", "Rodriguez", "Martinez" };

		public async Task GenerateAdjustmentRows(IList<CashDiscountRowDto> Adjustments, IEnumerable<string> reasonCodes, string adjustmentActivity)
		{
			if (!configService.IsOnTestRequest()) return;

			await Task.Run(() =>
			{
				Random rand = new Random();
				double min = 1000.00;
				double max = 10000000.00;
				int reasonCodeLastIndex = reasonCodes.Count() - 1;
				for (int x = 0; x < rand.Next(1, 10); x++)
				{
					float discountValue = (float)rand.NextDouble();

					var invoiceDetails = new InvoiceDetailsDto();
					string first = firstNames[rand.Next(firstNames.Length)];
					string last = lastNames[rand.Next(lastNames.Length)];

					invoiceDetails.InvoiceAmount = rand.NextDouble() * (max - min) + min;
					invoiceDetails.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					invoiceDetails.InvoiceDate = DateTime.Now.AddDays(-rand.Next(10, 200));
					invoiceDetails.CustomerName = first + " " + last;
					invoiceDetails.CustomerNumber = "09123456789";

					var row = new CashDiscountRowDto(
						discountValue,
						$"{(discountValue * 100).ToString("N2")}% Discount Remarks",
						reasonCodes.ElementAt(rand.Next(reasonCodeLastIndex)),
						adjustmentActivity,
						invoiceDetails
					);

					Adjustments.Add(row);
				}
			});
		}

		public async Task GenerateAdjustmentRows(IList<BankChargeRowDto> Adjustments, IEnumerable<string> reasonCodes, string adjustmentActivity)
		{
			if (!configService.IsOnTestRequest()) return;
			int reasonCodeLastIndex = reasonCodes.Count() - 1;

			await Task.Run(() =>
			{
				Random rand = new Random();
				double min = 10.00;
				double max = 10000000.00;

				for (int x = 0; x < rand.Next(1, 10); x++)
				{

					var invoiceDetails = new InvoiceDetailsDto();
					string first = firstNames[rand.Next(firstNames.Length)];
					string last = lastNames[rand.Next(lastNames.Length)];

					invoiceDetails.InvoiceAmount = rand.NextDouble() * (max - min) + min;
					invoiceDetails.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					invoiceDetails.InvoiceDate = DateTime.Now.AddDays(-rand.Next(10, 200));
					invoiceDetails.CustomerName = first + " " + last;
					invoiceDetails.CustomerNumber = "09123456789";

					float adjustmentAmount = (float)(rand.NextDouble() * (invoiceDetails.InvoiceAmount - 0) + 0);

					var row = new BankChargeRowDto(adjustmentAmount, $"Charged {adjustmentAmount.ToPhp()}", reasonCodes.ElementAt(rand.Next(reasonCodeLastIndex)), adjustmentActivity, invoiceDetails);

					Adjustments.Add(row);
				}
			});
		}

		public async Task GenerateAdjustmentRows(IList<SmallAmountRowDto> Adjustments, IEnumerable<string> reasonCodes, string adjustmentActivity)
		{
			if (!configService.IsOnTestRequest()) return;
			int reasonCodeLastIndex = reasonCodes.Count() - 1;

			await Task.Run(() =>
			{
				Random rand = new Random();
				double min = 10.00;
				double max = 10000000.00;

				for (int x = 0; x < rand.Next(1, 10); x++)
				{

					var invoiceDetails = new InvoiceDetailsDto();
					string first = firstNames[rand.Next(firstNames.Length)];
					string last = lastNames[rand.Next(lastNames.Length)];

					invoiceDetails.InvoiceAmount = rand.NextDouble() * (max - min) + min;
					invoiceDetails.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					invoiceDetails.InvoiceDate = DateTime.Now.AddDays(-rand.Next(10, 200));
					invoiceDetails.CustomerName = first + " " + last;
					invoiceDetails.CustomerNumber = "09123456789";

					float adjustmentAmount = (float)(rand.NextDouble() * (invoiceDetails.InvoiceAmount - 0) + 0);

					var row = new SmallAmountRowDto(adjustmentAmount, "SMALL AMOUNT", reasonCodes.ElementAt(rand.Next(reasonCodeLastIndex)), adjustmentActivity, invoiceDetails);

					Adjustments.Add(row);
				}
			});
		}

		public async Task GenerateAdjustmentRows(IList<SRAutoNetRowDto> Adjustments, IEnumerable<string> reasonCodes, string adjustmentActivity)
		{
			if (!configService.IsOnTestRequest()) return;
			int reasonCodeLastIndex = reasonCodes.Count() - 1;

			await Task.Run(() =>
			{
				Random rand = new Random();
				double min = 10.00;
				double max = 10000000.00;

				for (int x = 0; x < rand.Next(1, 10); x++)
				{

					var invoiceDetails = new InvoiceDetailsDto();
					string first = firstNames[rand.Next(firstNames.Length)];
					string last = lastNames[rand.Next(lastNames.Length)];

					invoiceDetails.InvoiceAmount = rand.NextDouble() * (max - min) + min;
					invoiceDetails.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					invoiceDetails.InvoiceDate = DateTime.Now.AddDays(-rand.Next(10, 200));
					invoiceDetails.CustomerName = first + " " + last;
					invoiceDetails.CustomerNumber = "09123456789";
					var remarks = new List<SRAutoNetRemarksDto>();

					for (int y = 0; y < rand.Next(1, 5); y++)
					{
						var _remarks = new SRAutoNetRemarksDto
						{
							CNRef = rand.Next(20_000, 9_000_000).ToString(),
							CNAmt = (float)(rand.NextDouble() * (invoiceDetails.InvoiceAmount - 0) + 0),
							WT = (float)(rand.NextDouble() * (invoiceDetails.InvoiceAmount - 0) + 0),
						};

						remarks.Add(_remarks);
					}

					var row = new SRAutoNetRowDto(reasonCodes.ElementAt(rand.Next(reasonCodeLastIndex)), remarks, invoiceDetails);

					Adjustments.Add(row);
				}
			});
		}
		
		public async Task GenerateAdjustmentRows(IList<APAROffsetAPRowDto> apGroup, IList<APAROffsetARRowDto> arGroup, IEnumerable<string> reasonCodes, string adjustmentActivity)
		{
			if (!configService.IsOnTestRequest()) return;
			int reasonCodeLastIndex = reasonCodes.Count() - 1;

			await Task.Run(() =>
			{
				Random rand = new Random();
				double min = 10.00;
				double max = 10000000.00;

				// 3,000
				// 7,000
				// sum = 10,000
				for (int x = 0; x < rand.Next(1, 5); x++)
				{

					var invoiceDetails = new InvoiceDetailsDto();
					string first = firstNames[rand.Next(firstNames.Length)];
					string last = lastNames[rand.Next(lastNames.Length)];

					invoiceDetails.InvoiceAmount = rand.NextDouble() * (max - min) + min;
					invoiceDetails.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					invoiceDetails.InvoiceDate = DateTime.Now.AddDays(-rand.Next(10, 200));
					invoiceDetails.CustomerName = first + " " + last;
					invoiceDetails.CustomerNumber = "09123456789";

					var row = new APAROffsetAPRowDto(invoiceDetails);

					apGroup.Add(row);
				}

				double sum = apGroup.Sum(a => a.InvoiceAmount);

				// count = 5
				// 1 = 1,000
					// sum = 9,000
				// 2 = 2,000
					// sum = 7,000
				// 3 = 2,000
					// sum = 5,000
				// 4 = 3,500
					// sum = 1,500
				// 5 = 1,500
					// sum = 9,000
				int arCount = rand.Next(1, 10);
				for (int x = 0; x < arCount; x++)
				{

					var row = new APAROffsetARRowDto();
					row.RowType = (ARRowType)rand.Next(0, 2);
					if (x == arCount - 1)
						row.Amount = sum;
					else
						row.Amount = rand.NextDouble() * (sum - 0) + 0;

					sum -= row.Amount;

					if (row.RowType == ARRowType.Invoice)
						row.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					else
						row.AdjustmentReason = reasonCodes.ElementAt(rand.Next(reasonCodeLastIndex));
					
					arGroup.Add(row);
				}
			});
		}
	}
}
