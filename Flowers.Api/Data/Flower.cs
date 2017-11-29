using System;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace Flowers.Api.Data
{
    public class Flower
    {
        public Flower()
        {
            Comments = new ObservableCollection<Comment>();
        }

        public bool HasChanges
        {
            get;
            set;
        }

        [JsonProperty("comments")]
        public ObservableCollection<Comment> Comments { get; set; }

        [JsonProperty("description")]
        public string Description
        {
            get;
            set;
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }

        public void AddComment(string comment)
        {
            Comments.Add(
                new Comment
                {
                    Id = Guid.NewGuid().ToString(),
                    InputDate = DateTime.Now,
                    Text = comment
                });
        }

        public void DeleteComment(string id)
        {
            var comment = Comments.FirstOrDefault(c => c.Id == id);
            if (comment != null)
            {
                Comments.Remove(comment);
            }
        }
    }
}