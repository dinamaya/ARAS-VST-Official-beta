using ARAS.Blazor.App_Code.Globals;
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

					invoiceDetails.InvoiceAmount = Utils.RandomGenerator.GetDouble(rand, max, min);
					invoiceDetails.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					invoiceDetails.InvoiceDate = Utils.RandomGenerator.GetEarlyDateTime(rand, 200, 10);
					invoiceDetails.CustomerName = first + " " + last;
					invoiceDetails.CustomerNumber = "09123456789";

					var row = new CashDiscountRowDto(
						discountValue,
						$"{(discountValue * 100).ToString("N2")}% Discount Remarks",
						Utils.RandomGenerator.GetElement(reasonCodes, rand, reasonCodeLastIndex),
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

					invoiceDetails.InvoiceAmount = Utils.RandomGenerator.GetDouble(rand, max, min);
					invoiceDetails.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					invoiceDetails.InvoiceDate = Utils.RandomGenerator.GetEarlyDateTime(rand, 200, 10);
					invoiceDetails.CustomerName = first + " " + last;
					invoiceDetails.CustomerNumber = "09123456789";

					float adjustmentAmount = Utils.RandomGenerator.GetFloat(rand, invoiceDetails.InvoiceAmount);

					string _reasonCode = Utils.RandomGenerator.GetElement(reasonCodes, rand, reasonCodeLastIndex);
					var row = new BankChargeRowDto(adjustmentAmount, $"Charged {adjustmentAmount.ToPhp()}", _reasonCode, adjustmentActivity, invoiceDetails);

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

					invoiceDetails.InvoiceAmount = Utils.RandomGenerator.GetDouble(rand, max, min);
					invoiceDetails.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					invoiceDetails.InvoiceDate = Utils.RandomGenerator.GetEarlyDateTime(rand, 200, 10);
					invoiceDetails.CustomerName = first + " " + last;
					invoiceDetails.CustomerNumber = "09123456789";

					float adjustmentAmount = Utils.RandomGenerator.GetFloat(rand, invoiceDetails.InvoiceAmount);

					string _reasonCode = Utils.RandomGenerator.GetElement(reasonCodes, rand, reasonCodeLastIndex);
					var row = new SmallAmountRowDto(adjustmentAmount, "SMALL AMOUNT", _reasonCode, adjustmentActivity, invoiceDetails);

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

					invoiceDetails.InvoiceAmount = Utils.RandomGenerator.GetDouble(rand, max, min);
					invoiceDetails.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					invoiceDetails.InvoiceDate = Utils.RandomGenerator.GetEarlyDateTime(rand, 200, 10);
					invoiceDetails.CustomerName = first + " " + last;
					invoiceDetails.CustomerNumber = "09123456789";

					var remarks = new List<SRAutoNetRemarksDto>();
					int remarksCount = rand.Next(1, 5);
					double sum = invoiceDetails.InvoiceAmount;

					for (int y = 0; y < remarksCount; y++)
					{
						var _remarks = new SRAutoNetRemarksDto
						{
							CNRef = rand.Next(20_000, 9_000_000).ToString(),
							CNAmt = Utils.RandomGenerator.GetDouble(rand, invoiceDetails.InvoiceAmount),
							WT = (x == remarksCount - 1) ? sum : Utils.RandomGenerator.GetDouble(rand, sum, min)
						};

						sum -= _remarks.WT;

						remarks.Add(_remarks);
					}

					string _reasonCode = Utils.RandomGenerator.GetElement(reasonCodes, rand, reasonCodeLastIndex);
					var row = new SRAutoNetRowDto(_reasonCode, remarks, invoiceDetails);

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

				for (int x = 0; x < rand.Next(1, 5); x++)
				{

					var invoiceDetails = new InvoiceDetailsDto();
					string first = firstNames[rand.Next(firstNames.Length)];
					string last = lastNames[rand.Next(lastNames.Length)];

					invoiceDetails.InvoiceAmount = Utils.RandomGenerator.GetDouble(rand, max, min);
					invoiceDetails.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					invoiceDetails.InvoiceDate = Utils.RandomGenerator.GetEarlyDateTime(rand, 200, 10);
					invoiceDetails.CustomerName = first + " " + last;
					invoiceDetails.CustomerNumber = "09123456789";

					var row = new APAROffsetAPRowDto(invoiceDetails);

					apGroup.Add(row);
				}

				double sum = apGroup.Sum(a => a.InvoiceAmount);

				int arCount = rand.Next(1, 10);
				for (int x = 0; x < arCount; x++)
				{

					var row = new APAROffsetARRowDto
					{
						RowType = (ARRowType)rand.Next(0, 2),
						Amount = (x == arCount - 1) ? sum : Utils.RandomGenerator.GetDouble(rand, sum, min)
					};

					sum -= row.Amount;

					if (row.RowType == ARRowType.Invoice)
						row.InvoiceNumber = rand.Next(20_000, 9_000_000).ToString();
					else
						row.AdjustmentReason = Utils.RandomGenerator.GetElement(reasonCodes, rand, reasonCodeLastIndex);

					arGroup.Add(row);
				}
			});
		}
	}
}
