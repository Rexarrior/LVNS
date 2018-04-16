namespace RobotsInterfaces
{
    /*
    public class RobotAddress
    {
        private string _mac;
        private IPEndPoint _addres;

        public RobotAddress(string mac, IPEndPoint addres)
        {
            _mac = mac;
            _addres = addres;
        }

        public string Mac { get => _mac; set => _mac = value; }

        public IPEndPoint Addres { get => _addres; set => _addres = value; }

        public override bool Equals(object obj)
        {
            var addres = obj as RobotAddress;
            return addres != null &&
                   _mac == addres._mac &&
                   EqualityComparer<IPEndPoint>.Default.Equals(_addres, addres._addres);
        }

        public override int GetHashCode()
        {
            var hashCode = -1100187958;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_mac);
            hashCode = hashCode * -1521134295 + EqualityComparer<IPEndPoint>.Default.GetHashCode(_addres);
            return hashCode;
        }

        public override string ToString()
        {
            return String.Format("mac: {0}  ; ip: {1}  ;  port: {2}", _mac, Addres.Address, Addres.Port) ;
        }
    }

*/

    public class Command
    {
        string _id;
        byte[] _message;

        public Command(string id, byte[] command)
        {
            _id = id;
            _message = command;
        }

        public string Id { get => _id; set => _id = value; }
        public byte[] Message { get => _message; set => _message = value; }
    }





    
}
