using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace se_remote {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            var thServer = new System.Threading.Thread(ServerRunner);
            thServer.Start();
        }

        private void WriteLine(string line) {
            this.textBox2.Invoke(new Action(() => this.textBox2.AppendText(line + "\r\n")));
        }

        private System.IO.Stream stream;

        private void ServerRunner() {
            var ipAd = System.Net.IPAddress.Parse("192.168.5.221");
            var listener = new System.Net.Sockets.TcpListener(ipAd, 8001);
            listener.Start();

            while (true) {

                System.Net.IPEndPoint ep = null;
                try {
                    using (var client = listener.AcceptTcpClient()) {
                        ep = (System.Net.IPEndPoint)client.Client.RemoteEndPoint;

                        this.WriteLine($"client {ep.Address} connected\r\n");

                        var blocks = new List<IEnumerable<byte>>(100);
                        var buf = new byte[4096];
                        using (this.stream = client.GetStream()) {
                            while (true) {
                                if (stream.Read(buf, 0, 4) != 4) {
                                    break;
                                }

                                int nLength = BitConverter.ToInt32(buf, 0);
                                int nread;

                                for (nread = 0; nread < nLength;) {
                                    int tread = stream.Read(buf, 0, Math.Min(buf.Length, nLength));
                                    if (tread == 0) {
                                        nread = -1;
                                        break;
                                    }
                                    nread += tread;

                                    var block = buf.Take(tread);
                                    blocks.Add(block);
                                    buf = new byte[buf.Length];
                                }

                                if (nread < 0) break;

                                var data = blocks.SelectMany(x => x);
                                this.WriteLine(Encoding.UTF8.GetString(data.ToArray()));
                                blocks.Clear();
                            }
                        }

                    }
                } catch (Exception ex) {
                    this.WriteLine($"server exception {ex.ToString()}\r\n");
                }

                this.WriteLine($"client {ep.Address} disconnected\r\n");
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            List<string> refs = new List<string>();
            foreach (var line in this.textBox1.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None)) {
                if (line.StartsWith("//! ")) {
                    refs.Add(line.Substring(3).Trim());
                }
            }

            var msg = X.ToXml(string.Join("\n", this.textBox1.Text), refs.ToArray());
            this.stream.Write(msg, 0, msg.Length);
        }

        private void button2_Click(object sender, EventArgs e) {
            this.textBox2.Clear();
        }
    }
}
