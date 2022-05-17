namespace SmartStore.BlobStorageService.Common
{
    public class CommonContains
    {
        public static List<ImageSize> ImageSize()
        {
            return new List<ImageSize>()
                        {
                            new ImageSize("small", 400, 250)
                        };
        }

        public static List<string> AllowedImgExt = "png,jpg,jpeg".Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        public static List<string> AllowedFileExt = "doc,docx,pdf".Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
