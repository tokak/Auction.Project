namespace Auction.Business.Dtos
{
    public class RegisterRequestDto
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
    }
}
