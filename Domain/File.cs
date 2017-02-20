using System;
using servicedesk.Common.Domain;

namespace servicedesk.Services.Drive.Domain
{
    public class File : IdentifiableEntity, ITimestampable
    {
        public string Resource { get; set; }
        public Guid ReferenceId { get; set; }
        public string Name { get; set; }
        public string FileType { get; set; }
        public string ContentType { get; set; }
        public double Size { get; set; }
        public FileContent Content { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class FileContent : IdentifiableEntity
    {
        public byte[] Content { get; set; }

        public FileContent() {}
        public FileContent(byte[] content)
        {
            Content = content;
        }
    }
}