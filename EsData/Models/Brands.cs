using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;



namespace EsData.Models
{
    public class Brands : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ID { get; set; }

        public string BrandName { get; set; }

        public int EthicalScore { get; set; }

        public string Link1 { get; set; }

        public string Link2 { get; set; }

        public int? ImageFileId { get; set; }

      
        private ImageFile imageFile;
        public ImageFile ImageFile
        {
            get
            {
                return this.imageFile;
            }
            set
            {
                this.imageFile = value;
                if (this.ImageFile != null)
                {
                    this.ImageFileId = this.ImageFile.Id;
                }
                else
                {
                    this.ImageFileId = null;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageFile)));
            }
        }
    
        public string ModifiedByUserId { get; set; }
    }
}