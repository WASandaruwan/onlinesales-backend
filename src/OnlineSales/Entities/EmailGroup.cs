﻿// <copyright file="EmailGroup.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineSales.DataAnnotations;

namespace OnlineSales.Entities
{
    [Table("email_group")]
    [SupportsChangeLog]
    public class EmailGroup : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name of the email group.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Language { get; set; } = string.Empty;
    }
}