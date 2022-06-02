using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
namespace NETWORK_SERVER
{
    internal class Program
    {
        public static int index = 0;
        public static Socket[] server = new Socket[100];  //등록된 클라이언트 갯수
        public static int[] ConnectCount = new int[100];  //등록된 클라이언트 갯수
        public static string[] sqldata;
        static void Main(string[] args)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 9000);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Bind(ip);
                socket.Listen(100);
                
                /* 아래 반복문은 서버 접속 코드다. 클라이언트가 앱을 종료시 -1을 반환하도록 설계할 것이고 새로운 클라이언트가 접속되면 -1이 반환된 배열부터 채운다
                 * -1이 없으면 index배열부터 채운다. 접속 가능한 클라리언트 갯수를  제한하기 위해서 이렇게 설계했다.*/

                while(true)
                {
                    int i = 0;
                    Console.WriteLine("접속 대기중...");
                    for(i = 0; i< ConnectCount.Length;i++)
                    {
                        if(ConnectCount[i] == -1)
                        {
                            server[i] = socket.Accept();
                            ConnectCount[i] = i;
                            new Thread(delegate () { Receiver(ConnectCount[i]); }).Start();
                            break;
                        }
                        if(i == ConnectCount.Length-1)
                        {
                            if(ConnectCount[i] != -1)
                            {
                                server[index] = socket.Accept();
                                ConnectCount[index] = index;
                                new Thread(delegate () { Receiver(ConnectCount[index]); }).Start();
                                index++;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }




        }

        static void Receiver(int idx)
        {
            byte[] buffer = new byte[1024];
            Console.WriteLine(idx + "클라이언트 접속");
            클라이언트 이름 = sql서버에서 받아온 사용자 계정 이름.
            try
            {
                while (true)
                {
                    server[idx].Receive(buffer);
                    string data = "";
                    data = Encoding.Unicode.GetString(buffer);
                    Console.WriteLine(idx + data);

                    string mode = data.Substring(0, 3);
                    string realdata = data.Substring(3);




                    
                    
                    
                    /* 각종 기능 별 데이터 */



                    if(mode == "CLO") // 클라이언트 close 시 CLO 메세지 전달 -1 반환해서 서버에 접속한 Receiver[idx] 클라이언트가 종료되었음을 알림
                    {
                        ConnectCount[idx] = -1;
                        break; //쓰레드 종료
                    }
                    else if (mode == "MSG")
                    {
                        ChatRoom(data);
                    }
                    else if (mode == "LOG") //LOG login 로그인 버튼 눌렀을 때 mode 값
                    {
                        Login(mode, data, idx);
                    }
                    else if (mode == "ANA") //ANA AddNewAccount 회원가입 버튼을 눌렀을 때 mode 값
                    {
                        Login(mode, data, idx);
                    }
                    else if(mode == "NAS")
                    {
                        Login(mode, data, idx);
                    }

                }
            }
            catch (Exception ex)
            { }

        }






















        public static void DataBroadCast(byte[] mode, byte[] data, int idx)
        {
            byte[] temp = new byte[mode.Length + data.Length];
            Array.Copy(mode, 0, temp, 0, mode.Length);//mode 0부터 mode.length 만큼 temp 0~ 에 저장
            Array.Copy(data, 0, temp, mode.Length, data.Length); //mode 0~length 만큼 temp 0~ 에 저장

            server[idx].Send(temp);
        }

        public static void ChatRoom(string mode, string data, int idx)
        {
            // 필요 기능: 아이디, 대화록, 현재 시간, 실시간 보내고 받는 메세지, 약속(공지)
        }



        public static void Login(string mode, string data, int idx)
        {
            if (mode == "LOG")
            {
                string judge = LoginCheck(data, idx);            //로그인 성공 실패 기능

                if(judge == "ACL")
                {
                    server[idx].Send(Encoding.Unicode.GetBytes(judge));
                    string judge2 = BaseLoginAfter(data, idx);
                    if(judge == "NAC")
                    {
                        server[idx].Send(Encoding.Unicode.GetBytes(judge2));
                    }
                    else if(judge2 == null)
                    {
                        // 없음 신경 안써도 됨//
                    }

                }
                else if(judge == "FAL")
                {
                    server[idx].Send(Encoding.Unicode.GetBytes(judge));
                }
            }
            else if (mode == "ANA")
            {
                string judge = AddNewAccount(data, idx);         //회원가입 기능
                if(judge != null)
                    server[idx].Send(Encoding.Unicode.GetBytes(judge));
            }
            else if(mode == "NAS")
            {
                Register_NAS(data);
                BaseLoginAfter(data, idx);
            }
        }





        public static string BaseLoginAfter(string data, int idx)
        {
            //사용자 이름, 친구 목록, 접속해 있는 방 정보 sql서버에서 받아와서 클라이언트에 보내기

            
            using (MySqlConnection connection = new MySqlConnection(
 "Server=localhost;Port=3306;Database=sample;Uid=root;Pwd=root;"))
            {
                try
                {
                    connection.Open();
                    sqldata = data.Split(' ');

                    String selectQuery = "SELECT * FROM client;";
                    String selectQuery2 = "SELECT * FROM roomlist;";
                    String selectQuery3 = "SELECT * FROM friendslist;";
                    MySqlCommand command = new MySqlCommand(selectQuery, connection);
                    MySqlCommand command2 = new MySqlCommand(selectQuery2, connection);
                    MySqlCommand command3 = new MySqlCommand(selectQuery3, connection);
                    MySqlDataReader ReadData = command.ExecuteReader();
                    MySqlDataReader ReadData2 = command2.ExecuteReader();
                    MySqlDataReader ReadData3 = command3.ExecuteReader();
                    while (ReadData.Read()) 
                    {
                        if (ReadData["ClientId"].ToString() == sqldata[0])
                            if (ReadData["ClientAccounts"].ToString() != DBNull.Value.ToString())
                            {
                                string accountsinfo = ReadData["ClientAccounts"].ToString();
                                DataBroadCast(Encoding.Unicode.GetBytes("AIF"),Encoding.Unicode.GetBytes(accountsinfo), idx); // accountsinfo 계정 이름 보내기
                            }
                            else if (ReadData["ClientAccounts"].ToString() == DBNull.Value.ToString())
                            {
                                return "NAC"; //new accounts 사용자 정보가 없으므로 새로운 사용자를 등록합니다
                            }
                    }
                    int count = 0;
                    string roomdata = "";
                    while (ReadData2.Read()) 
                    {
                        if (ReadData2["Id"].ToString() == sqldata[0])
                        {
                            roomdata += (ReadData2["MyRoomList"].ToString() + " "); // 띄어쓰기 마다 데이터 하나씩 넣어놓음
                            count++;
                        }

                    }
                    roomdata = count.ToString() + " " + roomdata; // count로 변수에 들어간 문자 갯수가 몇개 인지 알리기 위함
                    DataBroadCast(Encoding.Unicode.GetBytes("MYL"), Encoding.Unicode.GetBytes("roomdata"), idx);
                    string friendsdata = "";
                    count = 0;
                    while (ReadData3.Read())
                    {
                        if (ReadData2["Id"].ToString() == sqldata[0])
                        {
                            friendsdata += (ReadData2["FriendsList"].ToString() + " "); // 띄어쓰기 마다 데이터 하나씩 넣어놓음
                            count++;
                        }
                    }
                    friendsdata = count.ToString() + " " + friendsdata;
                    DataBroadCast(Encoding.Unicode.GetBytes("FLT"), Encoding.Unicode.GetBytes("friendsdata"), idx);
                    connection.Close();


                }
                catch(Exception ex)
                {

                }
                return null;
            }

        }


        public static void Register_NAS(string data)
        {
            using (MySqlConnection connection = new MySqlConnection(
"Server=localhost;Port=3306;Database=sample;Uid=root;Pwd=root;"))
            {
                try
                {
                    connection.Open();
                    sqldata = data.Split(' ');
                    String insertQuery = "INSERT INTO client(ClientAccounts,ClientLocationX, ClientLocationY) " +
      "VALUES ('" + sqldata[0] + "','" + sqldata[1] + "','" + sqldata[2] + "');";
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);  // 잘 몰겠다
                    command.ExecuteNonQuery();

                    connection.Close();
                }
                catch (Exception ex)
                {

                }
            }
        }





        public static string AddNewAccount(String data, int idx)
        {
            /* data에서 아이디 비밀번호 분리
            * sql 서버에 아이디 비밀번호 저장
            * 만약 같은 아이디가 존재하는 경우 회원가입 실패 mode 클라이언트에 보내기
            * 회원가입 완료 시 성공 mode 클라이언트에 보내기*/
            using (MySqlConnection connection = new MySqlConnection(
             "Server=localhost;Port=3306;Database=sample;Uid=root;Pwd=root;"))
            // Database, Uid, Pwd 는 모두 Mysql 워크벤치에서 본인이 설정한 값으로 알맞게 맞춰야합니다.
            // Database는 테이블의 값이 아니고 스키마의 값입니다!
            {
                try
                {
                    connection.Open();

                    sqldata = data.Split(' '); // 띄어쓰기 된 것 나눠서 sqldata 배열에 하나씩 저장
                    String insertQuery = "INSERT INTO client(ClientId,ClientPwd) " +
                        "VALUES ('" + sqldata[0] + "','" + sqldata[1] + "');"; // sql데이터 베이스에 있는 ClientId와 ClientPwd에 sqldata[0] 과 sqldata[1]을 저장




                    MySqlCommand command = new MySqlCommand(insertQuery, connection);  // 잘 몰겠다
                    String selectQuery = "SELECT * FROM client;";                      // 잘 몰겠다
                    MySqlCommand command1 = new MySqlCommand(selectQuery, connection); // 잘 몰겠다
                    MySqlDataReader ReadData = command1.ExecuteReader();               // 잘 몰겠다
                    while (ReadData.Read())  //데이터 베이스에 안에 있는 데이터를 0부터 끝까지 한번 쭉 읽음
                    {
                        if (ReadData["ClientId"].ToString() == sqldata[0]) // 만약 ClientId가 sqldata[0]과 같다면?
                        {
                            Console.WriteLine("이미 가입된 아이디");
                            return "SAM"; //같을 때
                        }
                    }
                    command.ExecuteNonQuery();                                         // 잘 몰겠다


                    connection.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return null;
                }
                return "SNA"; //회원가입 성공
            }
        }

        public static string LoginCheck(String data, int idx)
        {
            /* data에서 아이디 비밀번호 분리
            sql 데이터베이스에서 아이디 비밀번호 값 받아오기
            data값과 sql데이터베이스 값 비교
            성공 또는 실패 mode 클라이언트에 보내기
            */
            using (MySqlConnection connection = new MySqlConnection(
             "Server=localhost;Port=3306;Database=sample;Uid=root;Pwd=root;"))
            {
                try
                {
                    Boolean Check = false;
                    connection.Open();

                    sqldata = data.Split(' ');

                    String selectQuery = "SELECT * FROM client;";
                    MySqlCommand command = new MySqlCommand(selectQuery, connection);
                    MySqlDataReader ReadData = command.ExecuteReader();

                    while (ReadData.Read())
                        if (ReadData["ClientId"].ToString() == sqldata[0] && ReadData["ClientPwd"].ToString() == sqldata[1])
                            Check = true;

                    connection.Close();
                    if (Check == true)
                    {
                        return "ACL"; //로그인 성공
                    }
                    else
                        return "FAL"; //로그인 실패


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                return null;
            }
        }
    }
}
