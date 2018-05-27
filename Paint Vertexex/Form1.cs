using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Paint_Vertexex
{
    public partial class Form1 : Form
    {

        String source;
        List<List<int>> sourceMatrix;

        Dictionary<List<int>, List<int>> sourceTable;
        List<List<List<int>>> answer;

        int matrixY;

        public Form1()
        {
            InitializeComponent();
            sourceMatrix = new List<List<int>>();
            answer = new List<List<List<int>>>();
        }

        private void sourceChooser_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 1 - Текст, 2 - Файл
            int index = sourceChooser.SelectedIndex;
            switch(index)
            {
                case 0:
                    rawSourcePanel.Visible = true;
                    break;

                case 1:
                    rawSourcePanel.Visible = false;
                    ShowFileTypeSource();
                    if (sourceFileDialog.FileName.Equals("Выберите файл")) return;
                    tabControl1.SelectTab(2);
                    ParseText();
                    FindCycles();
                    break;

                default:
                    MessageBox.Show("Выбран несуществующий источник данных");
                    return;
            }
        }

        private void ParseText()
        {
            matrixSolutionPanel.Controls.Clear();
            matrixSolutionPanel.Refresh();
            matrixY = 0;
            sourceMatrix = new List<List<int>>();
            answer = new List<List<List<int>>>();
            textBoxAnswer.Text = "Матрица решается";
            int n = 0;
            foreach(char c in source)
            {
                if(c.Equals('\n'))
                {
                    n++;
                }
            }
            n++;
            int index = 0;
            for(int i = 0; i < n; i++)
            {
                List<int> row = new List<int>();
                for (int j = 0; j < n; j++)
                {
                    while (index < source.Length && source[index] != '0' && source[index] != '1') index++;
                    row.Add(source[index] - 48);
                    index++;
                }
                sourceMatrix.Add(row);
            }
        }

        private void ShowFileTypeSource()
        {
            sourceFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            sourceFileDialog.ShowDialog();
            source = sourceFileDialog.FileName;
            if(source == "Выберите файл")
            {
                MessageBox.Show("Вы не выбрали файл");
                source = "";
                return;
            }
            if(!source.Contains(".txt"))
            {
                MessageBox.Show("Вы должны выбрать текстовый файл");
                source = "";
                return;
            }
            source = File.ReadAllText(sourceFileDialog.FileName);
        }

        private void rawSourceButton_Click(object sender, EventArgs e)
        {
            if(rawSourceText.TextLength == 0)
            {
                MessageBox.Show("Вы ввести исходные данные");
                return;
            }

            source = rawSourceText.Text;
            ParseText();
            FindCycles();
        }

        private void FindCycles()
        {
            DrawHeader();
            int max = sourceMatrix.Count;
            List<List<List<int>>> cycles = new List<List<List<int>>>();
            int boxY = 20;
            for (int i = 0; i < max; i++)
            {
                List<string> visualRow = new List<string>();
                for(int j = 0; j < i; j++)
                {
                    visualRow.Add("");
                }
                visualRow.Add("1");
                List<List<int>> row = new List<List<int>>();
                //1 элемент
                List<int> _single = new List<int>();

                int[] _answerRow = new int[_single.Count + 1];
                _single.CopyTo(_answerRow);
                _answerRow[_answerRow.Length - 1] = i;

                boxY = VisualizeRow(visualRow.ToArray(), _single, boxY, _answerRow);
                //1 элемент
                for (int j = i; j < max; j++)
                {
                    string[] copy = new string[visualRow.Count];
                    visualRow.CopyTo(copy);

                    int v = sourceMatrix[i][j];
                    if (v == 0)
                    {
                        List<int> single = new List<int>();
                        single.Add(j);
                        row.Add(single);
                        if (i == j) continue;
                        /*int[] answerRow = new int[single.Count + 1];
                        single.CopyTo(answerRow);
                        answerRow[answerRow.Length - 1] = i;

                        boxY = VisualizeRow(copy, single, boxY, answerRow);*/
                        int rowCount = row.Count;
                        for (int m = 0; m < rowCount; m++)
                        {
                            if (AreElementsIntersectioning(row[m], j))
                            {
                                if (row[m].Count == 1 && row[m][0] == j) continue;
                                List<int> full = new List<int>();
                                int rowmCount = row[m].Count;
                                for (int k = 0; k < rowmCount; k++)
                                {
                                    full.Add(row[m][k]);
                                }
                                if(full[0]!=j)full.Add(j);
                                row.Add(full);

                                int[] answerRow = new int[full.Count];
                                full.CopyTo(answerRow);

                                boxY = VisualizeRow(copy, full, boxY, answerRow);
                            }
                        }
                    }
                    else
                    {
                        List<int> single = new List<int>();
                        single.Add(j);
                        boxY = VisualizeRow(copy, single, boxY, new int[0]);
                    }
                }
                ClearSame(row, boxY);
                cycles.Add(row);
            }
            ClearSameWhole(cycles);
            PrintCycleAnswer(cycles, boxY);
            if(checkBox1.CheckState == CheckState.Unchecked)
            {
                ClearCycles(cycles);
                BuildTable(cycles);
                DrawFullMatrix();
                DecomposeTable(sourceTable, new List<List<int>>());
                PrintTableAnswer();
            }
        }

        private void DrawFullMatrix()
        {
            RichTextBox title = new RichTextBox();
            title.Location = new Point(0, 0);
            matrixY += 25;
            title.Size = new Size(540, 20);
            title.Text = "Полученная матрица: ";
            matrixSolutionPanel.Controls.Add(title);
            for(int j = 0; j <= sourceMatrix.Count; j++)
            {
                TextBox textBox = new TextBox();
                textBox.Location = new Point(j * 50, matrixY);
                textBox.Size = new Size(50, 50);
                textBox.Text = j.ToString();
                matrixSolutionPanel.Controls.Add(textBox);
            }
            matrixY += 20;
            int i;
            foreach (List<int> key in sourceTable.Keys)
            {
                i = 0;
                TextBox textBox = new TextBox();
                textBox.Location = new Point((i++) * 50, matrixY);
                textBox.Size = new Size(50, 50);
                string keyStr = "{";
                foreach(int k in key)
                {
                    keyStr += (k + 1) + ", ";
                }
                textBox.Text = keyStr + "}";
                matrixSolutionPanel.Controls.Add(textBox);

                foreach(int v in sourceTable[key])
                {
                    TextBox textBox2 = new TextBox();
                    textBox2.Location = new Point((i++) * 50, matrixY);
                    textBox2.Size = new Size(50, 50);
                    textBox2.Text = v.ToString();
                    matrixSolutionPanel.Controls.Add(textBox2);
                }

                matrixY += 20;
            }
            matrixSolutionPanel.Refresh();
        }

        private void DrawMatrix(Dictionary<List<int>, List<int>> workMatrix, string message)
        {
            if (workMatrix.Count == 0)
            {
                RichTextBox _title = new RichTextBox();
                _title.Location = new Point(0, matrixY);
                matrixY += 20;
                _title.Size = new Size(540, 20);
                _title.Text = "Матрица была полностью истощена";
                matrixSolutionPanel.Controls.Add(_title);
                return;
            }
            RichTextBox title = new RichTextBox();
            title.Location = new Point(0, matrixY);
            matrixY += 20;
            title.Size = new Size(540, 20);
            title.Text = message;
            matrixSolutionPanel.Controls.Add(title);
            for (int j = 0; j <= workMatrix.ElementAt(0).Value.Count; j++)
            {
                TextBox textBox = new TextBox();
                textBox.Location = new Point(j * 50, matrixY);
                textBox.Size = new Size(50, 50);
                textBox.Text = j.ToString();
                matrixSolutionPanel.Controls.Add(textBox);
            }
            matrixY += 20;
            int i;
            foreach (List<int> key in workMatrix.Keys)
            {
                i = 0;
                TextBox textBox = new TextBox();
                textBox.Location = new Point((i++) * 50, matrixY);
                textBox.Size = new Size(50, 50);
                string keyStr = "{";
                foreach (int k in key)
                {
                    keyStr += (k + 1) + ", ";
                }
                textBox.Text = keyStr + "}";
                matrixSolutionPanel.Controls.Add(textBox);

                foreach (int v in workMatrix[key])
                {
                    TextBox textBox2 = new TextBox();
                    textBox2.Location = new Point((i++) * 50, matrixY);
                    textBox2.Size = new Size(50, 50);
                    textBox2.Text = v.ToString();
                    matrixSolutionPanel.Controls.Add(textBox2);
                }

                matrixY += 20;
            }
            matrixSolutionPanel.Refresh();
        }

        private void DrawBranching(string _message, List<List<int>> remembered)
        {
            RichTextBox title = new RichTextBox();
            title.Location = new Point(0, matrixY);
            matrixY += 20;
            title.Size = new Size(540, 20);

            string message = "{";

            foreach(List<int> a in remembered)
            {
                message += "{";
                foreach(int b in a)
                {
                    message += (b + 1).ToString() + ", ";
                }
                message += "}, ";
            }
            message += "}";
            title.Text = _message + message;
            matrixSolutionPanel.Controls.Add(title);
        }

        private void PrintTableAnswer()
        {
            int shortest = answer[0].Count;
            foreach (List<List<int>> rememberPacks in answer)
            {
                    if (rememberPacks.Count < shortest) shortest = rememberPacks.Count;
            }

            richTextBoxTableAnswer.Text = "Получено " + answer.Count + " вариантов раскраски вершин: \n";
            foreach(List<List<int>> rememberPacks in answer)
            {
                string packStr = "{ ";
                foreach(List<int> pack in rememberPacks)
                {
                    packStr += " {";
                    foreach(int i in pack)
                    {
                        packStr += (i + 1) + ", ";
                    }
                    packStr += "}, ";
                }
                if (rememberPacks.Count == shortest)
                {
                    packStr += "} - кратчайшая\n";
                }
                else
                {

                    packStr += "}\n";
                }
                richTextBoxTableAnswer.Text += packStr;
            }
        }

        private void DecomposeTable(Dictionary<List<int>, List<int>> workTable, List<List<int>> remembered)
        {            
            Dictionary<List<int>, List<int>> copy = MakeCopy(workTable);
            bool done = false;
            do
            {
                done = false;
                PrintMatrix(copy);
                if (copy.Count ==0 || copy.ElementAt(0).Value.Count < 2)
                {
                    Console.WriteLine("Answer is: ");
                    foreach (List<int> row in copy.Keys)
                    {
                        remembered.Add(row);
                        /*if(row.Count!=0)
                        {
                            Console.Write("{");
                            foreach(int el in row)
                            {
                                Console.Write("{0}, ", (el+1));
                            }
                            Console.Write("}, ");
                        }*/
                    }
                    Console.WriteLine("Before cleaning: ");
                    PrintRow(remembered);
                    ClearSameVertexes(remembered);
                    Console.WriteLine("Cleared: ");
                    PrintRow(remembered);
                    return;
                }
                List<int> foundCoreTables;
                List<List<int>> coreRaws = FindCoreTables(copy, out foundCoreTables);
                if (coreRaws.Count != 0)
                {
                    Console.Write("Core tables removed and saved:");
                    foreach (List<int> coreRaw in coreRaws)
                    {
                        remembered.Add(coreRaw);
                        Console.Write("{ ");
                        foreach (int a in coreRaw)
                        {
                            Console.Write("{0}, ", (a + 1));
                        }
                        Console.Write("}, ");
                    }
                    Console.WriteLine();
                    PrintMatrix(copy);
                    string deleted = "Удалены ядерные cтолбцы: ";
                    foreach(int a in foundCoreTables)
                    {
                        deleted += (a + 1) + ", ";
                    }
                    DrawMatrix(copy, deleted);
                    DrawBranching("Сохраненные циклы: ", remembered);
                    done = true;
                }
                List<List<int>> removedEmptyRaws;
                if (RemoveEmptyRaws(copy, out removedEmptyRaws))
                {
                    Console.WriteLine("Empty raws removed:");
                    PrintMatrix(copy);
                    string removed = "Удалены пустые строки: ";
                    foreach(List<int> a in removedEmptyRaws)
                    {
                        removed += "{";
                        foreach(int b in a)
                        {
                            removed += (b + 1) + ", ";
                        }
                        removed += "}, ";
                    }
                    DrawMatrix(copy, removed);
                    done = true;
                }
                List<int> removedEmptyTables;
                if (RemoveEmptyTables(copy, out removedEmptyTables))
                {
                    Console.WriteLine("Empty tables removed:");
                    PrintMatrix(copy);
                    string deleted = "Удалены пустые cтолбцы: ";
                    foreach (int a in removedEmptyTables)
                    {
                        deleted += (a + 1) + ", ";
                    }
                    DrawMatrix(copy, deleted);
                    done = true;
                }
                List<int> removedAbsTables;
                if (RemoveAbsorptioningTables(copy, out removedAbsTables))
                {
                    Console.WriteLine("Absorptioning tables removed:");
                    PrintMatrix(copy);
                    string deleted = "Удалены поглощающие cтолбцы: ";
                    foreach (int a in removedAbsTables)
                    {
                        deleted += (a + 1) + ", ";
                    }
                    DrawMatrix(copy, deleted);
                    done = true;
                }
                List<List<int>> removedAbsRows;
                if (RemoveAbsorptionedRows(copy, out removedAbsRows))
                {
                    Console.WriteLine("Absorptioned rows removed:");
                    PrintMatrix(copy);
                    string removed = "Удалены поглощаемые строки: ";
                    foreach (List<int> a in removedAbsRows)
                    {
                        removed += "{";
                        foreach (int b in a)
                        {
                            removed += (b + 1) + ", ";
                        }
                        removed += "}, ";
                    }
                    DrawMatrix(copy, removed);
                    done = true;
                }
                Console.WriteLine("Cycle finished");
            } while (done);
            int index = FindShortestTable(copy);
            for(int j = 0; j < copy.Keys.Count; j++)
            {
                List<int> key = copy.ElementAt(j).Key;
                List<int> row = copy[key];
                if (row[index] == 1)
                {
                    Dictionary<List<int>, List<int>> copyTabled = MakeCopy(copy);

                    List<int> tabledRow = copyTabled.ElementAt(j).Value;
                    int sizeT = copyTabled.ElementAt(0).Value.Count;
                    for (int i = 0; i < sizeT; i++)
                    {
                        if (tabledRow[i] == 1)
                        {
                            foreach (List<int> rR in copyTabled.Values)
                            {
                                rR.RemoveAt(i);
                            }
                            i = -1;
                            sizeT = copyTabled.ElementAt(0).Value.Count;
                        }
                    }
                    copyTabled.Remove(key);
                    Console.WriteLine("Creating new one on row {0}", (index+1));
                    List<List<int>> copyFinal = MakeCopyS(remembered);
                    copyFinal.Add(key);
                    DrawBranching("Новое ветвление: ", copyFinal);
                    DecomposeTable(copyTabled, copyFinal);
                }
            }
        }

        private void ClearSameVertexes(List<List<int>> remembered)
        {
            for (int i = remembered.Count-1; i >= 0; i--)
            {
                List<int> checkering = remembered[i];
                for (int j = 0; j < checkering.Count; j++)
                {
                    for (int k = remembered.Count-1; k >= 0; k--)
                    {
                        bool removed = false;
                        if (k == i) continue;
                        List<int> checker = remembered[k];
                        foreach(int vertex in checker)
                        {
                            if(checkering[j] == vertex)
                            {
                                checkering.RemoveAt(j);
                                removed = true;
                                break;
                            }
                        }
                        if (removed)
                        {
                            j = -1;
                            break;
                        }
                    }
                }
            }
            answer.Add(remembered);
        }

        private List<List<int>> MakeCopyS(List<List<int>> remembered)
        {
            List<List<int>> copy = new List<List<int>>();

            foreach(List<int> a in remembered)
            {
                List<int> buf = new List<int>();
                foreach(int b in a)
                {
                    buf.Add(b);
                }
                copy.Add(buf);
            }

            return copy;
        }

        private void PrintMatrix(Dictionary<List<int>, List<int>> copy)
        {
            /*for(int i = 0; i < copy.Keys.Count; i++)
            {
                Console.Write("{0} ", i);
            }*/

            foreach(List<int> key in copy.Keys)
            {
                Console.Write("{");
                foreach (int k in key)
                {
                    Console.Write("{0}, ", (k+1));
                }
                Console.Write("}: ");
                Console.WriteLine(string.Join(", ", copy[key].ToArray()));
            }
            Console.WriteLine();
        }
        

        private Dictionary<List<int>, List<int>> MakeCopy(Dictionary<List<int>, List<int>> copy)
        {
            Dictionary<List<int>, List<int>> a = new Dictionary<List<int>, List<int>>();
            foreach(KeyValuePair<List<int>, List<int>> pair in copy)
            {
                List<int> key = new List<int>();
                foreach(int i in pair.Key)
                {
                    key.Add((i));
                }
                List<int> value = new List<int>();
                foreach (int i in pair.Value)
                {
                    value.Add((i));
                }
                a.Add(key, value);
            }
            return a;
        }

        private int FindShortestTable(Dictionary<List<int>, List<int>> workTable)
        {
            int onesMin = int.MaxValue;
            int bestIndex = 0;
            int tables = workTable.ElementAt(0).Value.Count;
            for (int j = 0; j < tables; j++)
            {
                int ones = 0;
                foreach(List<int> row in workTable.Values)
                {
                    if (row[j] == 1) ones++;
                }
                if (ones < onesMin)
                {
                    onesMin = ones;
                    bestIndex = j;
                }
            }
            return bestIndex;
        }

        private bool RemoveAbsorptionedRows(Dictionary<List<int>, List<int>> workTable, out List<List<int>> removedAbsRows)
        {
            removedAbsRows = new List<List<int>>();
            if (workTable.Count == 0) return false;
            bool done = false;
            int rows = workTable.Values.Count;
            for (int j = 0; j < rows; j++)
            {
                bool shouldRestart = false;
                for (int m = 0; m < rows; m++)
                {
                    if (m == j) continue;
                    if (SecondRawAbsorbFirst(workTable, j, m))
                    {
                        removedAbsRows.Add(workTable.ElementAt(j).Key);
                        workTable.Remove(workTable.ElementAt(j).Key);
                        shouldRestart = true;
                        break;
                    }
                }
                if (shouldRestart)
                {
                    done = true;
                    rows = workTable.Values.Count;
                    j = -1;
                }
            }
            return done;
        }

        private bool SecondRawAbsorbFirst(Dictionary<List<int>, List<int>> workTable, int firstRaw, int secondRaw)
        {
            List<int> row1 = workTable.ElementAt(firstRaw).Value;
            List<int> row2 = workTable.ElementAt(secondRaw).Value;
            for (int i = 0; i < workTable.ElementAt(0).Value.Count; i++)
            {
                if (row2[i]<row1[i]) return false;
            }
            return true;
        }

        private bool RemoveAbsorptioningTables(Dictionary<List<int>, List<int>> workTable, out List<int> removedAbsTables)
        {
            removedAbsTables = new List<int>();
            if (workTable.Count == 0) return false;
            bool done = false;
            int tables = workTable.ElementAt(0).Value.Count;
            int removed = 0;
            for (int j = 0; j < tables; j++)
            {
                bool shouldRestart = false;
                for (int m = j+1; m < tables; m++)
                {
                    if(SecondTableAbsorbFirst(workTable, j, m))
                    {
                        foreach (List<int> row in workTable.Values)
                        {
                            row.RemoveAt(m);
                        }
                        removedAbsTables.Add(m+removed);
                        removed++;
                        shouldRestart = true;
                        break;
                    }
                }
                if(shouldRestart)
                {
                    done = true;
                    tables = workTable.ElementAt(0).Value.Count;
                    j = -1;
                }
            }
            return done;
        }

        private bool SecondTableAbsorbFirst(Dictionary<List<int>, List<int>> workTable, int firstT, int secondT)
        {
            foreach(List<int> row in workTable.Values)
            {
                if (row[firstT] != 0 && row[firstT] != row[secondT]) return false;
            }
            return true;
        }

        private bool RemoveEmptyTables(Dictionary<List<int>, List<int>> workTable, out List<int> removedEmptyTables)
        {
            removedEmptyTables = new List<int>();
            if (workTable.Count == 0) return false;
            bool done = false;
            int size = workTable.ElementAt(0).Value.Count;
            int removed = 0;
            for (int i = 0; i < size; i++)
            {
                int empty = 0;
                foreach (List<int> row in workTable.Values)
                {
                    if (row[i] != 0)
                    {
                        empty++;
                        break;
                    }                    
                }
                if (empty == 0)
                {
                    done = true;
                    removedEmptyTables.Add(i + removed);
                    foreach (List<int> row in workTable.Values)
                    {
                        row.RemoveAt(i);
                    }
                    i = -1;
                    removed++;
                    size = workTable.ElementAt(0).Value.Count;
                }
            }
            return done;
        }

        private bool RemoveEmptyRaws(Dictionary<List<int>, List<int>> workTable, out List<List<int>> removedEmptyRaws)
        {
            removedEmptyRaws = new List<List<int>>();
            if (workTable.Count == 0) return false;
            bool done = false;
            int rows = workTable.Values.Count;
            for (int i = 0; i < rows; i++)
            {
                List<int> row = workTable.ElementAt(i).Value;
                int empty = 0;
                foreach(int el in row)
                {
                    if(el != 0)
                    {
                        empty++;
                        break;
                    }
                }
                if(empty==0)
                {
                    done = true;
                    removedEmptyRaws.Add(workTable.ElementAt(i).Key);
                    workTable.Remove(workTable.ElementAt(i).Key);
                    i = -1;
                    rows = workTable.Values.Count;
                }
            }
            return done;
        }

        private List<List<int>> FindCoreTables(Dictionary<List<int>, List<int>> workTable, out List<int> deletedCoreTables)
        {
            deletedCoreTables = new List<int>();
            if (workTable.Count == 0) return new List<List<int>>();
            List<List<int>> coreRaws = new List<List<int>>();
            int size = workTable.ElementAt(0).Value.Count;
            int delta = 0;
            int removed = 0;
            for(int i = 0; i < size; i++)
            {
                int oneAmount = 0;
                int rowIndex = 0;
                for(int l = 0; l < workTable.Values.Count; l++)
                {
                    List<int> row = workTable.ElementAt(l).Value;
                    if (row[i] == 1)
                    {
                        rowIndex = l;
                        oneAmount++;
                    }
                }
                if(oneAmount == 1)
                {
                    deletedCoreTables.Add(i + removed++);
                    coreRaws.Add(workTable.ElementAt(rowIndex).Key);
                    int index = 0;
                    List<int> removedRow = workTable.ElementAt(rowIndex).Value;
                    //int removedAmount = 0;
                    for (int l = 0; l < removedRow.Count; l++)
                    {
                        int el = removedRow[l];
                        if (el == 1)
                        {
                            foreach (List<int> row in workTable.Values)
                            {
                                row.RemoveAt(index);
                            }
                            l = -1;
                            index = -1;
                            //removedAmount++;
                        }
                        index++;
                    }
                    workTable.Remove(workTable.ElementAt(rowIndex).Key);
                    /*foreach (List<int> row in workTable.Values)
                    {
                        row.RemoveAt(i- removedAmount);
                    }*/
                    delta++;
                    i = -1;
                    if (workTable.Count == 0) break;
                    size = workTable.ElementAt(0).Value.Count;
                }
            }

            return coreRaws;
        }

        private void ClearCycles(List<List<List<int>>> cycles)
        {
            foreach (List<List<int>> cycle in cycles.ToArray())
            {
                foreach (List<int> row in cycle.ToArray())
                {
                    if (row.Count == 0) cycle.Remove(row);
                }
                if (cycle.Count == 0) cycles.Remove(cycle);
            }
        }

        private void BuildTable(List<List<List<int>>> cycles)
        {
            sourceTable = new Dictionary<List<int>, List<int>>();
            for (int i = 0; i < cycles.Count; i++)
            {
                List<List<int>> cycle = cycles[i];
                List<int> row;
                for (int j = 0; j < cycle.Count; j++)
                {
                    row = new List<int>();
                    for (int m = 0; m < sourceMatrix.Count; m++)
                    {
                        if(cycle[j].Contains(m))
                        {
                            row.Add(1);
                        } else
                        {
                            row.Add(0);
                        }
                    }

                    sourceTable.Add(cycle[j], row);
                }                
            }
        }

        private void PrintCycleAnswer(List<List<List<int>>> cycles, int boxY)
        {
            /*RichTextBox textBox = new RichTextBox();
            textBox.Location = new Point(0, boxY);
            textBox.Size = new Size(583, 50);*/

            string answer = "Получено "+cycles.Count+" циклов: ";

            foreach(List<List<int>> cycle in cycles)
            {
                foreach (List<int> row in cycle)
                {
                    if (row.Count == 0) continue;
                    answer += "{";
                    foreach (int el in row)
                    {
                        answer += (el+1) + ", ";
                    }
                    answer += "}, ";
                }
            }

            textBoxAnswer.Text = answer;
            panelAnswer.Refresh();
        }

        private int VisualizeRow(string[] copy, List<int> points, int boxY, int[] answerRow)
        {
            int i;
            for (i = 0; i < copy.Length; i++)
            {
                TextBox textBox = new TextBox();
                textBox.Location = new Point(i * 50, boxY);
                textBox.Size = new Size(50, 50);
                textBox.Text = copy[i];
                panelSolution.Controls.Add(textBox);
            }

            for (; i < sourceMatrix.Count; i++)
            {
                TextBox textBox = new TextBox();
                textBox.Location = new Point(i * 50, boxY);
                textBox.Size = new Size(50, 50);
                for (int j = 0; j < points.Count; j++)
                {
                    if(i==points[j])
                    {
                        textBox.Text = "1";
                        break;
                    } else
                    {
                        textBox.Text = "";
                    }
                }
                panelSolution.Controls.Add(textBox);
            }

            /*foreach(int vertex in points)
            {
                TextBox textBox = new TextBox();
                textBox.Location = new Point(vertex * 50, boxY);
                textBox.Size = new Size(50, 50);
                textBox.Text = "1";
                panelSolution.Controls.Add(textBox);
            }*/
            string answer = "Нельзя объединить";
            if (answerRow.Length>0)
            {
                answer = "{";
                for (int j = 0; j < answerRow.Length; j++)
                {
                    answer += (answerRow[j] + 1) + ", ";
                }
                answer += "}";
            }

            TextBox textBox2 = new TextBox();
            textBox2.Location = new Point(sourceMatrix.Count * 50, boxY);
            textBox2.Size = new Size(150, 50);
            textBox2.Text = answer;
            panelSolution.Controls.Add(textBox2);

            panelSolution.Refresh();

            boxY+=20;
            return boxY;
        }

        private void DrawHeader()
        {
            int i;
            for (i = 0; i < sourceMatrix.Count; i++)
            {
                TextBox textBox = new TextBox();
                textBox.Location = new Point(i*50, 0);
                textBox.Size = new Size(50, 50);
                textBox.Text = (i+1).ToString();
                panelSolution.Controls.Add(textBox);
            }
            TextBox textBox2 = new TextBox();
            textBox2.Location = new Point(i * 50, 0);
            textBox2.Size = new Size(50, 50);
            textBox2.Text = "";
            panelSolution.Controls.Add(textBox2);
        }

        private void ClearSameWhole(List<List<List<int>>> cycles)
        {
            for(int i = 0; i < cycles.Count; i++)
            {
                List<List<int>> row = cycles[i];
                for (int j = 0; j < row.Count; j++)
                {
                    List<int> cycle = row[j];
                    for (int m = 0; m < cycles.Count; m++)
                    {
                        if (m != i)
                        {
                            row[j] = ClearSameInRow(cycles[m], cycle);
                            int l;
                            for(l = 0; l < row[j].Count; l++)
                            {
                                if (row[j][l] != -1) break;
                            }
                            if (l == row[j].Count) break;
                        }
                    }
                }
                PrintRow(row);
            }
        }

        private List<int> ClearSameInRow(List<List<int>> row, List<int> cycle)
        {
            for (int j = 0; j < row.Count; j++)
            {
                int[] copy = new int[cycle.Count];
                cycle.CopyTo(copy);
                if (copy.Length > row[j].Count) continue;
                for (int k = 0; k < row[j].Count; k++)
                {
                    if (copy.Contains(row[j][k]))
                    {
                        for (int l = 0; l < copy.Length; l++)
                        {
                            if (copy[l] == row[j][k])
                            {
                                copy[l] = -1;
                                break;
                            }
                        }
                    }
                }
                int m;
                for (m = 0; m < copy.Length; m++)
                {
                    if (copy[m] != -1)
                    {
                        break;
                    }
                }
                if (m == copy.Length)
                {
                    return new List<int> ();
                }
            }
            return cycle;
        }

        private int ClearSame(List<List<int>> row, int boxY)
        {
            List<int> vertexesToRemove = new List<int>();
            for(int i = 0; i < row.Count; i++)
            {
                /*string rowResult = "Рассматриваем набор вершин {";

                List<int> rowi = row[i];
                for (int j = 0; j < rowi.Count; j++)
                {
                    int ans = (rowi[j] + 1);
                    rowResult += ans;
                    rowResult += (j == rowi.Count - 1) ? "}" : ", ";
                }

                rowResult += ": ";

                TextBox textBox = new TextBox();
                textBox.Location = new Point(0, boxY);
                textBox.Size = new Size(50, 50);*/

                for (int j = 0; j < row.Count; j++)
                {
                    int[] copy = new int[row[i].Count];
                    row[i].CopyTo(copy);
                    if (copy.Length > row[j].Count || i == j) continue;
                    for (int k = 0; k < row[j].Count; k++)
                    {
                        if (copy.Contains(row[j][k]))
                        {
                            for (int l = 0; l < copy.Length; l++)
                            {
                                if (copy[l] == row[j][k])
                                {
                                    copy[l] = -1;
                                    break;
                                }
                            }
                        }
                    }
                    int m;
                    for (m = 0; m < copy.Length; m++)
                    {
                        if (copy[m] != -1)
                        {
                            break;
                        }
                    }
                    if (m == copy.Length)
                    {
                        vertexesToRemove.Add(i);
                        break;
                    }
                }
            }
            int z = 0;
            foreach(int vert in vertexesToRemove)
            {
                row.RemoveAt(vert-z);
                z++;
            }
            return boxY;
        }

        private void PrintRow(List<List<int>> row)
        {
            foreach(List<int> els in row)
            {
                if (els.Count == 0) continue;
                Console.Write("{");
                foreach(int el in els)
                {
                    Console.Write("{0}, ", el+1);
                }
                Console.Write("}, ");
            }
            Console.WriteLine();
        }

        private bool AreElementsIntersectioning(List<int> element, int j)
        {
            foreach(int el in element)
            {
                if (sourceMatrix[el][j] == 1) return false;
            }
            return true;
        }
    }
}
