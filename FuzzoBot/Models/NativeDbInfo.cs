// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

using System.Collections.Generic;

public class Prop
{
    public string? type { get; set; }
    public string? name { get; set; }
    public object? flags { get; set; }
    public PageProps? pageProps { get; set; }
    public bool __N_SSG { get; set; }
}

public class Param
{
    public string? type { get; set; }
    public string? name { get; set; }
    public int flags { get; set; }
}

public class Return
{
    public string? type { get; set; }
    public int flags { get; set; }
}

public class Func
{
    public string? fullName { get; set; }
    public string? shortName { get; set; }
    public int flags { get; set; }
    public List<Param>? @params { get; set; }
    public Return? @return { get; set; }
}

public class Data
{
    public string? parent { get; set; }
    public string? name { get; set; }
    public int flags { get; set; }
    public List<Prop>? props { get; set; }
    public List<Func>? funcs { get; set; }
}

public class PageProps
{
    public string? title { get; set; }
    public int type { get; set; }
    public Data? data { get; set; }
}

public class Query
{
    public string? native { get; set; }
}

public class Root
{
    public Prop? props { get; set; }
    public string? page { get; set; }
    public Query? query { get; set; }
    public string? buildId { get; set; }
    public bool isFallback { get; set; }
    public bool gsp { get; set; }
    public List<object>? scriptLoader { get; set; }
}