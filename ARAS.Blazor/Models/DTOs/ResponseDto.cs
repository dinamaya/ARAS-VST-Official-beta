namespace ARAS.Blazor.Models.DTOs
{
  public class ResponseDto
  {
    public object? Result { get; set; } = null;
    public bool IsSuccess { get; set; } = true;
    public string Message { get; set; } = string.Empty;
  }

  public class ResponseDto<T> : ResponseDto
  {
    public new T? Result
    {
            get
            {
                try
                {
                    if (base.Result == null)
                    {
                        return default(T);
                    }
                    return (T)base.Result;
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
            set => base.Result = value;
    }
  }
}
