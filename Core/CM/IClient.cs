using System;
namespace Core
{
    namespace CM
    {
        interface IClient
        {
            private int id;
            public int ID {get; set;}
            private string userType;
            public string UserType { get; set; }
        }
    }
}

