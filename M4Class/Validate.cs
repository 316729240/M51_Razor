using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Web;

namespace MWMS
{

    public class Validate
    {
        public Validate()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        public void CreateValidate(System.Web.UI.Page containsPage)
        {
            string checkCode = this.GetCode(CodeLength);
            System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)Math.Ceiling((double)(5 * 17)), 22);//画布
            Graphics g = Graphics.FromImage(image);

            try
            {
                Random random = new Random();
                g.Clear(Color.White); //图片背景色

                /**/
                ///画图片的背景噪音线
                for (int i = 0; i < 5; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }

                Font font = new System.Drawing.Font("Arial", 12);
                System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.White, Color.White, 1.2f, true);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                /**/
                ///生成不同颜色字符的图片
                for (Int32 i = 0; i < 5; i++)
                {

                    Brush b = new System.Drawing.SolidBrush(Color.FromArgb(random.Next(160), random.Next(160), random.Next(160)));
                    Int32 ii = 4;
                    if ((i + 1) % 2 == 0)
                    {
                        ii = 2;
                    }
                    g.DrawString(checkCode.Substring(i, 1), font, b, 2 + (i * 14), ii);
                }

                /**/
                ///画图片的前景噪音点
                for (int i = 0; i < 20; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);

                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }

                /**/
                ///画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                containsPage.Response.ClearContent();
                containsPage.Response.ContentType = "image/Gif";
                containsPage.Response.BinaryWrite(ms.ToArray());
                ms.Close();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }
        public String GetCode(Int32 length)
        {
            String[] source ={"2","3","4","5","6","7","8","9",
            "a","b","c","d","e","f","g","h","i","j","k","m","n","p","q","r","s","t","u","v","w","x","y","z",
            "A","B","C","D","E","F","G","H","I","J","K","M","N","P","Q","R","S","T","U","V","W","X","Y","Z"};

            //因为数字0,1和字母O,i不好认，所以我没有用到这几个！
            String code = "";
            Random rd = new Random();
            for (Int32 i = 0; i < length; i++)
            {
                code += source[rd.Next(0, source.Length)];
            }
            return code;
        }
        private System.Int32 CodeLength
        {
            get
            {
                System.Int32 length;
                if (Int32.TryParse(HttpContext.Current.Request.QueryString["length"], out length))
                {
                    if (length > 5)
                    {
                        return length;
                    }
                }
                return 5;
            }
        }
    }
}
