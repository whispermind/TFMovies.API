﻿using System.ComponentModel.DataAnnotations.Schema;
using TFMovies.API.Common.Constants;

namespace TFMovies.API.Data.Entities;

public class PostComment : BaseModel
{
    [ForeignKey("Post")]
    public string PostId { get; set; }

    [ForeignKey("User")]
    public string UserId { get; set; }

    public string Content { get; set; }

    public DateTime CreatedAt { get; set; }

    public Post Post { get; set; }

    public User User { get; set; }   
}
