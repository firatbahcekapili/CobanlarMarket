//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CobanlarMarket.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class addresses
    {
        public int id { get; set; }
        public Nullable<int> user_id { get; set; }
      
        [Required(ErrorMessage = "Ad alan� gereklidir")]
        [StringLength(20, ErrorMessage = "Ad en fazla 20 karakter olmal�d�r")]
        public string name { get; set; }

        [Required(ErrorMessage = "Soyad alan� gereklidir")]
        [StringLength(30, ErrorMessage = "Soyad en fazla 30 karakter olmal�d�r")]
        public string surname { get; set; }

        [StringLength(100, ErrorMessage = "Ba�l�k en fazla 100 karakter olabilir")]
        public string title { get; set; }

        [Required(ErrorMessage = "Adres alan� gereklidir")]
        [StringLength(200, ErrorMessage = "Adres en fazla 200 karakter olabilir")]
        public string address { get; set; }

        [Required(ErrorMessage = "�lke alan� gereklidir")]
        [StringLength(50, ErrorMessage = "�lke ad� en fazla 50 karakter olmal�d�r")]
        public string country { get; set; }

        [Required(ErrorMessage = "�ehir alan� gereklidir")]
        [StringLength(50, ErrorMessage = "�ehir ad� en fazla 50 karakter olmal�d�r")]
        public string city { get; set; }

        [Required(ErrorMessage = "Posta kodu alan� gereklidir")]
        [RegularExpression(@"\d{5}", ErrorMessage = "Posta kodu 5 rakamdan olu�mal�d�r")]
        public string postal_code { get; set; }

        [StringLength(100, ErrorMessage = "Yer i�areti (landmark) en fazla 100 karakter olabilir")]
        public string landmark { get; set; }

        [Required(ErrorMessage = "Telefon numaras� alan� gereklidir")]
        [Phone(ErrorMessage = "Ge�erli bir telefon numaras� giriniz")]
        public string phone_number { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public Nullable<System.DateTime> deleted_at { get; set; }
    
        public virtual users users { get; set; }
    }
}
