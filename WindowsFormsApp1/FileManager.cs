using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;

namespace WindowsFormsApp1
{
    class FileManager
    {
        public FileManager(string currentPath)
        {
            this.appPath = currentPath;
            this.jsonpath = currentPath + @"\test.json";

            Initial_Folder();
        }

        private string appPath;
        private string jsonpath;

        private void Initial_Folder()
        {
            StreamReader file = File.OpenText(jsonpath);
            JsonTextReader reader = new JsonTextReader(file);

            JObject json = (JObject)JToken.ReadFrom(reader);

            XmlManager xm = new XmlManager();

            xm.CreateODT();

            xm.SetPageLayout();

            make(xm, json);

            xm.SaveODT(xm.root);

            reader.Close();
            file.Close();

            //xm.Create_Document(appPath + loadName);

            //xm.Update_Text("Test Test one two one two");

            //xm.Save_Document(appPath + fileName);
        }

        private void Initial_XML()
        {

        }

        private void make(XmlManager xm, JObject json)
        {
            // 문단 내 텍스트 별로 subcontent 만들기
            // p 생성
            for (int i = 0; i < json["BodyText"]["Section_0"]["HWPTAG_PARA_LINE_SEG"]["PARA LINE SEG"].Count(); i++)
            {
                // 문단 ID 가져오기
                int shapeID = json["BodyText"]["Section_0"]["PARAMETER_List"]["PARA_" + i + "_HWPTAG_PARA_HEADER"]["ShapeId"].Value<int>();
                string sID = shapeID.ToString();

                //전체 텍스트 가져오기
                string pcontent = null; // p text
                try
                {
                    object obj = json["BodyText"]["Section_0"]["HWPTAG_PARA_TEXT"]["PARA"]["PARA " + i].Value<object>();

                    if (obj != null)
                        pcontent = json["BodyText"]["Section_0"]["HWPTAG_PARA_TEXT"]["PARA"]["PARA " + i]["Text"].Value<string>();
                }
                catch (Exception e)
                {
                    xm.AddContentP(); // para null 인 경우 처리
                    continue;
                }

                // 스타일별 텍스트 개수
                int spancount = json["BodyText"]["Section_0"]["PARAMETER_List"]["PARA_" + i + "_HWPTAG_PARA_CHAR_SHAPE"]["PositonShapeIdPairList"].Count(); // p 안 text style count
                // 스타일별 텍스트 아이디
                int pstyle = json["BodyText"]["Section_0"]["PARAMETER_List"]["PARA_" + i + "_HWPTAG_PARA_CHAR_SHAPE"]["PositonShapeIdPairList"]["PositonShapeIdPairList_0"]["ShapeId"].Value<int>();

                int current_position;
                int next_position;

                string pname = "";
                string name;

                // 텍스트별 위치 비교해서 자르기
                for (int j = 0; j < spancount; j++)
                {
                    //스타일이 시작되는 위치
                    current_position = json["BodyText"]["Section_0"]["PARAMETER_List"]["PARA_" + i + "_HWPTAG_PARA_CHAR_SHAPE"]["PositonShapeIdPairList"]["PositonShapeIdPairList_" + j]["Position"].Value<int>();

                    string subcontent;
                    if (j < spancount - 1)
                    {
                        next_position = json["BodyText"]["Section_0"]["PARAMETER_List"]["PARA_" + i + "_HWPTAG_PARA_CHAR_SHAPE"]["PositonShapeIdPairList"]["PositonShapeIdPairList_" + (j + 1)]["Position"].Value<int>();
                        if (i == 0)
                        {
                            current_position = current_position - 16 < 0 ? 0 : current_position - 16;
                            next_position -= 16;
                        }
                        subcontent = pcontent.Substring(current_position, next_position - current_position); //처음포지션부터 글자 수만큼 자르기
                    }
                    else //마지막 텍스트
                    {
                        if (i == 0 && j != 0)
                        {
                            current_position -= 16;
                        }
                        subcontent = pcontent.Substring(current_position);
                    }

                    //스타일 추가 및 내용 추가
                    //스타일의 아이디
                    int currentstyle = json["BodyText"]["Section_0"]["PARAMETER_List"]["PARA_" + i + "_HWPTAG_PARA_CHAR_SHAPE"]["PositonShapeIdPairList"]["PositonShapeIdPairList_" + j]["ShapeId"].Value<int>();

                    /////////////////////////////////////
                    /*if (j == 0)
                        name = xm.AddContentP(subcontent);
                    else if (currentstyle == pstyle)
                        xm.AddContentP(i, subcontent); //pstyle과 같으면 텍스트만 추가
                    else
                        name = xm.AddContentSpan(pname, subcontent); //pstyle과 다르면 span 생성 후 텍스트 추가*/
                    ///////////////////////////////////////////////추가 수정 필요
                    if (spancount == 1)
                    {
                        pname = xm.AddContentP(pcontent);
                        name = pname;
                    }
                    else if (j == 0)
                    {
                        pname = xm.AddContentP("");
                        name = xm.AddContentSpan(pname, subcontent);
                    }
                    else
                        name = xm.AddContentSpan(pname, subcontent);
                    //


                    //스타일 속성 추가
                    //문단 속성 추가
                    if (j == 0)
                    {
                        //줄 간격
                        int lineSpace = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["LineSpace"].Value<int>();
                        xm.Paragraph.SetLineSpace(pname, (XmlDocument)xm.docs["content.xml"], lineSpace);

                        //문단 테두리 간격
                        double topborderSpace = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["TopBorderSpace"].Value<double>();
                        double bottomBorderSpace = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["BottomBorderSpace"].Value<double>();
                        double leftBorderSpace = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["LeftBorderSpace"].Value<double>();
                        double rightBorderSpace = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["RightBorderSpace"].Value<double>();

                        topborderSpace *= 0.01;
                        bottomBorderSpace *= 0.01;
                        leftBorderSpace *= 0.01;
                        rightBorderSpace *= 0.01;

                        xm.Paragraph.SetBorderSpace(pname, (XmlDocument)xm.docs["content.xml"], topborderSpace, bottomBorderSpace, leftBorderSpace, rightBorderSpace);

                        //줄 나눔
                        //한글이 ByWord일 경우 적용 (한글, 영어 모두)
                        string byWord = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["Property1"]["LineDivideForHangul"].Value<string>();
                        if (byWord.Equals("ByWord"))
                        {
                            xm.Paragraph.SetByWord(pname, (XmlDocument)xm.docs["content.xml"]);
                        }

                        //외톨이줄 보호 여부
                        bool isProtectLoner = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["Property1"]["isProtectLoner"].Value<bool>();
                        if (isProtectLoner)
                            xm.Paragraph.SetisProtectLoner(pname, (XmlDocument)xm.docs["content.xml"]);

                        //다음 문단과 함께 여부
                        bool isTogetherNextPara = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["Property1"]["isTogetherNextPara"].Value<bool>();
                        if (isTogetherNextPara)
                            xm.Paragraph.SetisTogetherNextPara(pname, (XmlDocument)xm.docs["content.xml"]);

                        //문단 보호 여부
                        bool isProtectPara = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["Property1"]["isProtectPara"].Value<bool>();
                        if (isProtectPara)
                            xm.Paragraph.SetisProtectPara(pname, (XmlDocument)xm.docs["content.xml"]);

                        //한글과 영어 간격을 자동 조절 여부
                        bool isAutoAdjustGapHangulEnglish = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["Property2"]["isAutoAdjustGapHangulEnglish"].Value<bool>();
                        if (isAutoAdjustGapHangulEnglish)
                            xm.Paragraph.SetisAutoAdjustGapHangulEnglish(pname, (XmlDocument)xm.docs["content.xml"]);


                        //정렬
                        string paraAlign = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["Property1"]["Alignment"].Value<string>();
                        if (paraAlign.Equals("Right"))
                            xm.SetPAlign(pname, "end");
                        else if (paraAlign.Equals("Justify"))
                            xm.SetPAlign(pname, "justify");
                        else if (paraAlign.Equals("Center"))
                            xm.SetPAlign(pname, "center");
                        else if(paraAlign.Equals("Divide"))
                            xm.SetPAlign(pname, "Divide");
                        else if(paraAlign.Equals("Distribute"))
                            xm.SetPAlign(pname, "Distribute");

                        //첫줄 들여쓰기
                        //margin
                        double indent = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["Indent"].Value<double>();
                        double topspace = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["TopParaSpace"].Value<double>();
                        double bottomspace = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["BottomParaSpace"].Value<double>();
                        double leftmargin = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["LeftMargin"].Value<double>();
                        double rightmargin = json["DocInfo 2"]["HWPTAG_PARA_SHAPE"]["PARA_SHAPE"]["PARA_SHAPE_" + sID]["RightMargin"].Value<double>();
                        if (indent != 0 || topspace != 0 || bottomspace != 0 || leftmargin != 0 || rightmargin != 0)
                        {
                            xm.SetPIndent(pname, (float)(indent / 200 * 0.0353));
                            if (indent < 0)
                                leftmargin = -indent + leftmargin;
                            xm.SetPMargin(pname, (float)(leftmargin / 200 * 0.0353), (float)(rightmargin / 200 * 0.0353), (float)(topspace / 200 * 0.0353), (float)(bottomspace / 200 * 0.0353));
                        }



                    }

                    bool bold = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["isBold"].Value<bool>();
                    bool italic = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["isItalic"].Value<bool>();
                    bool kerning = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["isKerning"].Value<bool>();
                    string underline = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["UnderLineSort"].Value<string>();
                    int fontcolor = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["CharColor"].Value<int>();
                    bool strikeline = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["isStrikeLine"].Value<bool>();
                    double baseSize = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["BaseSize"].Value<double>(); // pt * 100 값

                    if (true)
                    {
                        //진하게
                        if (bold)
                            xm.SetBold(name);
                        //기울임
                        if (italic)
                            xm.SetItalic(name);

                        //간격
                        if (kerning)
                        {
                            baseSize = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["BaseSize"].Value<int>(); // pt * 100 값
                            double kerningSpace = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["CharSpacebyLanguage"]["Hangul"].Value<int>(); // kerning percent 값

                            double value = baseSize * (kerningSpace / 100) / 100 * 0.0353; //pt로 환산 후 cm로 환산
                            xm.SetKerning(name, baseSize.ToString() + "cm");
                        }

                        //폰트 사이즈, 폰트 색
                        xm.SetFontSize(name, json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["BaseSize"].Value<float>() / 100);
                        if (fontcolor != 0)
                        {
                            byte[] bit = BitConverter.GetBytes(fontcolor);
                            Array.Reverse(bit);
                            fontcolor = BitConverter.ToInt32(bit, 0);
                            xm.SetFontColor(name, "#" + fontcolor.ToString("X8").Substring(0, 6));
                        }


                        //윗줄 밑줄
                        if (underline.Equals("Below Letters"))
                        {
                            int lineshape = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["UnderLineShape"].Value<int>();
                            int linecolor = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["UnderLineColor"].Value<int>();
                            byte[] bit = BitConverter.GetBytes(linecolor);
                            Array.Reverse(bit);
                            linecolor = BitConverter.ToInt32(bit, 0);
                            xm.SetUnderline(name, lineshape, "#" + linecolor.ToString("X8").Substring(0, 6));
                        }
                        else if (underline.Equals("Above Letters"))
                        {
                            int lineshape = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["UnderLineShape"].Value<int>();
                            int linecolor = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["UnderLineColor"].Value<int>();
                            byte[] bit = BitConverter.GetBytes(linecolor);
                            Array.Reverse(bit);
                            linecolor = BitConverter.ToInt32(bit, 0);
                            xm.SetUnderline(name, lineshape, "#" + linecolor.ToString("X8").Substring(0, 6));
                        }

                        //취소선 odt에서는 종류 제한, 색상 선택 불가
                        if (strikeline)
                        {
                            int lineshape = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["StrikeLineShape"].Value<int>();
                            xm.SetThroughline(name, lineshape);
                        }

                        //외곽선 한글에 종류 여러개지만 odt에서는 한개
                        if (!json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["OutterLineSort"].Value<string>().Equals("None"))
                        {
                            xm.SetOutline(name);
                        }

                        //그림자 odt한종류
                        if (!json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["ShadowSort"].Value<string>().Equals("None"))
                        {
                            xm.SetShadow(name);
                        }

                        //음각 양각
                        if (json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["isEmboss"].Value<bool>())
                        {
                            xm.SetRelief(name);
                        }
                        else if (json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["isEngrave"].Value<bool>())
                        {
                            xm.SetRelief(name, "engraved");
                        }

                        //위첨자
                        //아래첨자
                        if (json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["isSuperScript"].Value<bool>())
                        {
                            xm.SetSuper(name);
                        }
                        else if (json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["Property"]["isSubScript"].Value<bool>())
                        {
                            xm.SetSub(name);
                        }

                        //글꼴 적용

                        int fontID = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["FontNameidbyLanguage"]["Hangul"].Value<int>();
                        string fontName = json["DocInfo 2"]["HWPTAG_FACE_NAME"]["FONT_NAME"]["FONT_NAME_" + fontID.ToString()]["FaceName"].Value<string>();
                        xm.SetFont(name, fontName);

                        //글자간격
                        double charspace = json["DocInfo 2"]["HWPTAG_CHAR_SHAPE"]["CHAR_SHAPE"]["CHAR_SHAPE_" + currentstyle]["CharSpacebyLanguage"]["Hangul"].Value<double>();
                        if (charspace != 0)
                            xm.SetLetterSpace(name, (float)(baseSize * 0.01 * charspace * 0.01 * 0.0353));

                    }

                }
            }
        }
    }
}
