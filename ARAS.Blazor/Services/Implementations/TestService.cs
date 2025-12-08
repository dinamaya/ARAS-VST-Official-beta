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

		public async Task GenerateAdjustmentRows(IList<CashDiscountRowDto> Adjustments, IEnumerable<string> reasonCodes)
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
						invoiceDetails
					);

					Adjustments.Add(row);
				}
			});
		}

		public async Task GenerateAdjustmentRows(IList<BankChargeRowDto> Adjustments, IEnumerable<string> reasonCodes)
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

					var row = new BankChargeRowDto(adjustmentAmount, $"Charged {adjustmentAmount.ToPhp()}", reasonCodes.ElementAt(rand.Next(reasonCodeLastIndex)), invoiceDetails);

					Adjustments.Add(row);
				}
			});
		}

		public async Task GenerateAdjustmentRows(IList<SmallAmountRowDto> Adjustments, IEnumerable<string> reasonCodes)
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

					var row = new SmallAmountRowDto(adjustmentAmount, "SMALL AMOUNT", reasonCodes.ElementAt(rand.Next(reasonCodeLastIndex)), invoiceDetails);

					Adjustments.Add(row);
				}
			});
		}
	}
}
