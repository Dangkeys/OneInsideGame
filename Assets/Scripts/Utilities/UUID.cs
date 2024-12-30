using System;

public static class UUID
{
     public static string Create_ID()
     {
          return Guid.NewGuid().ToString();
     }
}