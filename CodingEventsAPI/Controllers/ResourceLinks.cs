using System;
using System.Net.Http;
using CodingEventsAPI.Models;
using Microsoft.Extensions.Configuration;

namespace CodingEventsAPI.Controllers {
  public class ResourceLink {
    public string Href { get; }

    public HttpMethod Method { get; }

    public static string ServerOrigin { get; set; }

    internal ResourceLink(string path, HttpMethod method) {
      Method = method;
      Href = $"{ServerOrigin}{path}";
    }
  }

  public struct CodingEventResourceLinks {
    public readonly Func<CodingEvent, ResourceLink> GetTags;
    public readonly Func<CodingEvent, ResourceLink> GetCodingEvent;
    public readonly Func<CodingEvent, ResourceLink> CancelCodingEvent;
    public readonly Func<CodingEvent, Tag, ResourceLink> AddTag;
    public readonly Func<CodingEvent, Tag, ResourceLink> RemoveTag;

    internal CodingEventResourceLinks(string entrypoint) {
      GetTags = codingEvent => new ResourceLink(
        $"{entrypoint}/{codingEvent.Id}/tags",
        HttpMethod.Get
      );

      GetCodingEvent = codingEvent => new ResourceLink(
        $"{entrypoint}/{codingEvent.Id}",
        HttpMethod.Get
      );

      CancelCodingEvent = codingEvent => new ResourceLink(
        $"{entrypoint}/{codingEvent.Id}",
        HttpMethod.Delete
      );

      AddTag = (codingEvent, tag) => new ResourceLink(
        $"{entrypoint}/{codingEvent.Id}/tags/{tag.Id}",
        HttpMethod.Put
      );

      RemoveTag = (codingEvent, tag) => new ResourceLink(
        $"{entrypoint}/{codingEvent.Id}/tags/{tag.Id}",
        HttpMethod.Delete
      );
    }
  }

  public struct MemberResourceLinks {
    public readonly Func<CodingEvent, ResourceLink> GetMembers;
    public readonly Func<CodingEvent, ResourceLink> JoinCodingEvent;
    public readonly Func<CodingEvent, ResourceLink> LeaveCodingEvent;
    public readonly Func<Member, ResourceLink> RemoveMember;

    internal MemberResourceLinks(string entrypoint) {
      GetMembers = codingEvent => new ResourceLink(
        $"{entrypoint}/{codingEvent.Id}/members",
        HttpMethod.Get
      );

      JoinCodingEvent = codingEvent => new ResourceLink(
        $"{entrypoint}/{codingEvent.Id}/members",
        HttpMethod.Post
      );

      LeaveCodingEvent = codingEvent => new ResourceLink(
        $"{entrypoint}/{codingEvent.Id}/members",
        HttpMethod.Delete
      );

      RemoveMember = member => new ResourceLink(
        $"{entrypoint}/{member.CodingEventId}/members/{member.Id}",
        HttpMethod.Delete
      );
    }
  }

  public struct TagResourceLinks {
    public readonly Func<Tag, ResourceLink> GetTag;
    public readonly Func<Tag, ResourceLink> GetCodingEvents;

    internal TagResourceLinks(string entrypoint) {
      GetTag = tag => new ResourceLink($"{entrypoint}/{tag.Id}", HttpMethod.Get);

      GetCodingEvents = tag => new ResourceLink($"{entrypoint}/{tag.Id}/events", HttpMethod.Get);
    }
  }
}
