using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSText
{
    class Program
    {
        static void Main()
        {
            string testText = @"[1] ASME Boiler and Pressure Vessel Code, Section 
VIII, The American Society of Mechanical Engineers, 
2004
[2] RCC MR Design Code, Section 1, French Society for
Design and Construction Rules for Nuclear Island 
Components, 2002
[3] R. SandströM, S.T. Tu, J. Pres. Ves. Technol., 116, 
76-80 (1994)
[4] S.-T. Tu, R. Sandström, Int. J. Pres. Ves. Piping, 57, 
335-344 (1994)
[5] M.D. Mathew, S. Latha, K.B.S. Rao, Mater. Sci. 
Eng. A, 456, 28-34 (2007) 
[2] RCC MR Design Code, Section 1, French Society for
Design and Construction Rules for Nuclear Island 
Components, 2002
[3] R. SandströM, S.T. Tu, J. Pres. Ves. Technol., 116, 
76-80 (1994)";

            string textSegmentSymbol = "\n\n";
            testText = TextSplit(testText, textSegmentSymbol);
            Console.OutputEncoding = Encoding.Unicode;
            Console.WriteLine(testText);
            Console.ReadLine();
        }

        public static string TextSplit(string testText, string textSegmentSymbol)
        {

            //var posts = new string[] { "post1", "post2", "post3", "post4", "post5", "post6", "post7", "post8",
            //    "post9", "post10" };
            //var slicedPosts = posts.Skip(1).Take(5);
            //foreach (var post in slicedPosts)
            //    Console.WriteLine(post); // Outputs the first 5 posts

            
            // 删除末位的换行符，常见于网页复制的文本末尾包含\r\n
            testText = Regex.Replace(testText, @"$[\n\r]", " ");
            // 删除空行 
            // https://stackoverflow.com/questions/7647716/how-to-remove-empty-lines-from-a-formatted-string
            testText = Regex.Replace(testText, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
            // 替换LaTeX中的连字符
            testText = Regex.Replace(testText, @"ﬁ", "fi").Replace(@"ﬃ", "ffi").Replace(@"ﬂ", "fl").Replace(@"ﬀ", "fl");
            // 替换容易导致导致翻译错误的缩写
            testText = Regex.Replace(testText, @"[Ee]qs?\.\s", "Eq."); //eq. 替换成 eq.(无空格)
            // 删除全角字符之间的空格
            testText = Regex.Replace(testText, @"(?<=[^\x00-\xff])[' ']+(?=[^\x00-\xff])", "");
            // 删除两个以上的空格
            testText = Regex.Replace(testText, @"[' ']+", " ");  // 连续空格替换为一个空格
            // 替换乱码项目符，常见于PDF中的·
            testText = Regex.Replace(testText, @"|", "-");

            // 按段落分割
            string[] testTextArray = testText.Split('\n');

            // 查看段落分割结果
            //foreach (var word in testTextArray)
            //{
            //    //Console.WriteLine(word);
            //}

            // 查看段落数量
            int testTextArrayLength = testTextArray.Length;
            //Console.WriteLine(testTextArrayLength);

            // 段落匹配规则
            string regexEndPunctuation = @"[\\.:;!。！？?：\s]$"; // 末位标点匹配
            string regexItem = @"^^•|^–\s|^-\s|^Chapter\s[1-9][0-9]{0,1}|^([1-9]*\.)+\d{1,2}[\.\s]|^·|^\[\d*\]\s[A-Z]|^\d\)\s|^\d）|^\d*\.\s|^\([Ii][Xx]\)|^\([Ii][Vv]\)|^\([Vv]\)|^\([Vv]?[Ii]{1,3}\)"; // 开头项目符号匹配
            string regexEndAbbr = @" fig\.$| et al\.$| Fig\.$| Eq\.$| eq\.$| p\.$| pp\.$| Ph\.D\.$|cf\.$|Cf\.$|,\s\d{4};$|\.\s\(\d{4}\);$|[Ee]\.[Gg]\.$"; // 末位缩写词
            string regexFirstCapital = @"^[A-Z]"; // 首位大写字母匹配，用于判断英文标题行
            string regexTitle = @"^[Aa][\s+]*[Bb][\s+]*[Ss][\s+]*[Tt][\s+]*[Rr][\s+]*[Aa][\s+]*[Cc][\s+]*[Tt]$|^[Aa]cknowledge?ments$|^[Rr]eferences$|^参[\s+]*考[\s+]*文[\s+]*献$|^致[\s+]*谢$|^附[\s+]*录$|^摘[\s+]*要$|^目[\s+]*录$|^[Dd]eclaration [Oo]f [Cc]ompeting [Ii]nterest$|^[Ii]ntroduction$"; // 匹配常见的论文标题
            bool[] isEnd = new bool[testTextArrayLength]; // 末尾是否结束标点，是则末尾分段
            bool[] isItem = new bool[testTextArrayLength]; // 开头是否项目符，是则开头分段
            bool[] isEndAbbr = new bool[testTextArrayLength]; // 末尾结束标点是否缩写词，是则取消末尾分段
            bool[] isEndFullWidth = new bool[testTextArrayLength]; // 末尾是否全角字符
            bool[] isStartFullWidth = new bool[testTextArrayLength]; // 开头是否全角字符， 两头都是全角字符不加空格，否则加空格链接，优先级低于末尾结束标点
            bool[] isFirstCapital = new bool[testTextArrayLength]; // 是否为首字母大写
            bool[] isTitle = new bool[testTextArrayLength]; // 是否为常见标题

            // 文本信息提取
            for (int index = 0; index < testTextArrayLength; index++)
            {
                // 删除前后的空白字符
                testTextArray[index] = testTextArray[index].Trim();
                // 匹配末位是否有结束标点
                isEnd[index] = Regex.IsMatch(testTextArray[index], regexEndPunctuation);
                //Console.WriteLine("末位标点 {0}", isEnd[index]);
                // 匹配开头项目符
                isItem[index] = Regex.IsMatch(testTextArray[index], regexItem);
                //Console.WriteLine("开头项目符 {0}", isItem[index]);
                // 匹配末尾缩写词
                isEndAbbr[index] = Regex.IsMatch(testTextArray[index], regexEndAbbr);
                //Console.WriteLine("末位缩写词 {0}", isEndAbbr[index]);
                // 匹配开头大写字符
                isFirstCapital[index] = Regex.IsMatch(testTextArray[index], regexFirstCapital);
                // 匹配是否为常见标题
                isTitle[index] = Regex.IsMatch(testTextArray[index], regexTitle);

                // 提取末位字符
                char lastCharaterTemp = testTextArray[index][testTextArray[index].Length - 1];
                string lastCharacter = lastCharaterTemp.ToString();
                //Console.WriteLine("末位字符 {0}", lastCharacter);
                // 末位字符的字节
                //Console.WriteLine(Encoding.Default.GetByteCount(lastCharacter));
                // 字节数为3，则为全角，字节数为1，则为半角
                if (Encoding.Default.GetByteCount(lastCharacter) >= 2)
                {
                    isEndFullWidth[index] = true;
                }
                else
                {
                    isEndFullWidth[index] = false;
                }

                // 提取首位字符
                char firstCharaterTemp = testTextArray[index][0];
                string firstCharacter = firstCharaterTemp.ToString();
                //Console.WriteLine("首位字符 {0}", firstCharacter);
                // 末位字符的字节
                //Console.WriteLine(Encoding.Default.GetByteCount(firstCharacter));
                // 字节数为3，则为全角，字节数为1，则为半角
                if (Encoding.Default.GetByteCount(firstCharacter) >= 2)
                {
                    isStartFullWidth[index] = true;
                }
                else
                {
                    isStartFullWidth[index] = false;
                }

            }

            // 文本处理逻辑
            string resultText = "";
            // 一行文本不需要处理，直接输出
            if (testTextArrayLength == 1)
            {
                resultText = testText;
                //Console.WriteLine(resultText);
            }
            else //多行文本开始执行分段逻辑
            {
                // 创建一个字符串数组，保存分段连接符，分三种情况，分段则为"\n"，空格连接则为" "，直接连接则为空""
                string[] delimiterArray = new string[testTextArrayLength];
                delimiterArray[testTextArrayLength - 1] = ""; // 为便于拼接，最有一个连接符为空

                // 进入文本判断循环
                for (int index = 0; index < testTextArrayLength - 1; index++)
                {
                    if (isEnd[index]) // 末尾为结束标点
                    {
                        if (isEndAbbr[index]) // 末尾为结束英文缩写，末尾为半角
                        {
                            if (isItem[index + 1]) //下一行开头是项目符
                            {
                                delimiterArray[index] = textSegmentSymbol;
                            }
                            else // 下一行开头不是项目符，项目符序列判断后续补充
                            {
                                delimiterArray[index] = " ";
                            }
                        }
                        else // 末尾不是英文缩写
                        {
                            delimiterArray[index] = textSegmentSymbol;
                        }
                    }
                    else // 末尾不是结束标点
                    {
                        if (isItem[index + 1]) // 下一行开头是项目符
                        {
                            delimiterArray[index] = textSegmentSymbol;
                        }
                        else // 下一行开头不是项目符，项目符序列判断后续补充
                        {
                            if (isEndFullWidth[index]) // 末尾是全角字符
                            {
                                if (isEndFullWidth[index + 1]) // 下一行开头是全角
                                {
                                    delimiterArray[index] = "";
                                }
                                else // 下一行开头是半角
                                {
                                    delimiterArray[index] = "";
                                }
                            }
                            else // 末尾是半角字符
                            {
                                delimiterArray[index] = " ";
                            }
                        }
                    }
                }

                // 若该行为标题，则前后为换行符，优先级最高
                for (int index = 0; index < testTextArrayLength; index++)
                {
                    if (index == 0 && isTitle[index] == true)
                    {
                        delimiterArray[index] = textSegmentSymbol;
                    }
                    else if (index == testTextArrayLength - 1 && isTitle[index] == true)
                    {
                        delimiterArray[index - 1] = textSegmentSymbol;
                    }
                    else if (isTitle[index] == true)
                    {
                        delimiterArray[index - 1] = textSegmentSymbol;
                        delimiterArray[index] = textSegmentSymbol;
                    }
                }

                // 判断标题行，若项目符位置与之之后的首字母大写位置之间全部为""连接，则在首字符大写行前面增加分段

                int[] itemPosition = Enumerable.Range(0, testTextArrayLength).Where(i => isItem[i] == true).ToArray(); // 找出项目符行位置
                int[] firstCapitalPosition = Enumerable.Range(0, testTextArrayLength).Where(i => isFirstCapital[i] == true).ToArray(); // 找出首字母大写行位置
                /*
                 * ===========item1==========item2=========
                 * ====caps1=========cpas2=======caps3======
                 * 下文为item 与 caps的位置判断逻辑，举例，如果item1与caps2之间的行末分隔数组全都是 ""(即无分段)，那就判断 caps2前一行为标题
                 * 但目前无法识别中文标题
                 * */

                for (int i = 0; i < itemPosition.Length; i++) // 先循环项目符标点
                {
                    bool isGeFirstCapital = false; // 是否匹配到下一行首字母大写
                    for (int j = 0; j < firstCapitalPosition.Length && !isGeFirstCapital; j++) // 匹配如果匹配到一次首字母大写就停止，从下一个项目符重新开始
                    {
                        if (itemPosition[i] < firstCapitalPosition[j])
                        {
                            if (i == itemPosition.Length - 1)
                            {
                                if (itemPosition[i] < firstCapitalPosition[j])
                                {
                                    var tmp = delimiterArray.Skip(itemPosition[i]).Take(firstCapitalPosition[j] - itemPosition[i]);
                                    // Quicker 不兼容C# 切片 .. 语法
                                    bool isBlankConnect = true;    // 是否为空格连接符
                                    foreach (var separator in tmp) //tmp)
                                    {
                                        if (separator != " ")
                                        {
                                            isBlankConnect = false;
                                            break;
                                        }
                                    }
                                    if (isBlankConnect)
                                    {
                                        delimiterArray[firstCapitalPosition[j] - 1] = textSegmentSymbol;
                                        isGeFirstCapital = true;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (itemPosition[i + 1] > firstCapitalPosition[j])
                                {
                                    if (itemPosition[i] < firstCapitalPosition[j])
                                    {
                                        var tmp = delimiterArray.Skip(itemPosition[i]).Take(firstCapitalPosition[j] - itemPosition[i]);
                                        // Quicker 对C# 切片 .. 语法不兼容
                                        bool isBlankConnect = true;    // 是否为空格连接符
                                        foreach (var separator in tmp)
                                        {
                                            if (separator != " ")
                                            {
                                                isBlankConnect = false;
                                                break;
                                            }
                                        }
                                        if (isBlankConnect)
                                        {
                                            delimiterArray[firstCapitalPosition[j] - 1] = textSegmentSymbol;
                                            isGeFirstCapital = true;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                // 判断是否为参考文献
                // 提取文本开头的序列(?<=^\[)\d*(?=\])，若为等差1的数列，则判断为参考文献条目。
                string regexRefSequence = @"(?<=^\[)\d*(?=\])"; // 匹配文字序列 [1][23]等
                int refSequenceNum = 0; // 获取匹配的数量
                for (int i = 0; i < testTextArrayLength; i++)
                {
                    if (Regex.IsMatch(testTextArray[i], regexRefSequence))
                    {
                        refSequenceNum++;
                    }
                }

                if (refSequenceNum > 1) // 序列数大于1，则判断为参考文献
                {
                    int refSequenceCount = 0;
                    int[] refSequenceArray = new int[refSequenceNum]; // 记录序列的数值
                    int[] refSequenceArrayPosition = new int[refSequenceNum]; // 记录序列的位置
                    for (int i = 0; i < testTextArrayLength; i++)
                    {
                        Match refSequenceMatch = Regex.Match(testTextArray[i], regexRefSequence);
                        if (refSequenceMatch.Success)
                        {
                            refSequenceArray[refSequenceCount] = Int32.Parse(refSequenceMatch.Value); // string to num
                            refSequenceArrayPosition[refSequenceCount] = i;
                            refSequenceCount++;
                        }
                    }
                    for (int i = 0; i < refSequenceArrayPosition.Length - 1; i++)
                    {
                        if (refSequenceArray[i + 1] - refSequenceArray[i] == 1 && refSequenceArrayPosition[i + 1] - refSequenceArrayPosition[i] <= 8) // 数字连续，且一条参考文献很少超过8行
                        {
                            for (int j = refSequenceArrayPosition[i]; j < refSequenceArrayPosition[i + 1] - 1; j++)
                            {
                                delimiterArray[j] = " ";
                            }
                        }
                    }

                }

                // 进入文本拼接
                string[] resultTextArray = new string[testTextArrayLength];
                for (int index = 0; index < testTextArrayLength; index++)
                {
                    resultTextArray[index] = testTextArray[index] + delimiterArray[index];
                }
                resultText = string.Join("", resultTextArray);

            }
            return resultText;
        }
    }
}
