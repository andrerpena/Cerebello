namespace CerebelloWebRole.Code
{
    public struct CreateThumbResult
    {
        private readonly CreateThumbStatus status;
        private readonly byte[] data;
        private readonly string contentType;

        public CreateThumbResult(CreateThumbStatus status, byte[] data, string contentType)
        {
            this.status = status;
            this.data = data;
            this.contentType = contentType;
        }

        /// <summary>
        /// Gets the resulting status of the operation.
        /// </summary>
        public CreateThumbStatus Status
        {
            get { return this.status; }
        }

        /// <summary>
        /// Gets an array containing the thumbnail image bytes.
        /// </summary>
        public byte[] Data
        {
            get { return this.data; }
        }

        /// <summary>
        /// Gets te content type of the data in the returned array.
        /// </summary>
        public string ContentType
        {
            get { return this.contentType; }
        }
    }
}