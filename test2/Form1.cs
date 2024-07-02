using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using Newtonsoft.Json;

namespace test2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.MinimumSize = new System.Drawing.Size(600, 400); // Установим минимальный размер окна
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Тест по информатике";
            this.Size = new System.Drawing.Size(800, 600); // Установим стартовый размер окна

            var startButton = new Button
            {
                Text = "Начать тест",
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Size = new System.Drawing.Size(200, 50),
                Location = new Point((this.ClientSize.Width - 200) / 2, (this.ClientSize.Height - 50) / 2),
                Anchor = AnchorStyles.None
            };
            startButton.Click += StartButton_Click;
            this.Controls.Add(startButton);
            this.Resize += (s, ev) => ResizeButton(startButton);
        }
        private void ResizeButton(Button button)
        {
            button.Location = new Point((this.ClientSize.Width - button.Width) / 2, (this.ClientSize.Height - button.Height) / 2);
        }
        private void StartButton_Click(object sender, EventArgs e)
        {
            FormTest formTest = new FormTest();
            formTest.Show();
            this.Hide();
        }
    }

    public class FormTest : Form
    {
        private List<Question> questions;
        private int currentQuestionIndex;
        private Button button1, button2, button3;
        private Button[] questionButtons;
        private Label label1;
        private Panel panel1;
        private ProgressBar progressBar;
        private Timer timer;
        private int timeLeft;

        public FormTest()
        {
            questions = LoadQuestions();
            currentQuestionIndex = 0;
            InitializeComponents();
            DisplayQuestion();
            StartTimer();
        }

        private List<Question> LoadQuestions()
        {
            var questionFiles = new string[] { "../../questions1.json", "../../questions2.json", "../../questions3.json", "../../questions4.json", "../../questions5.json" };
            var random = new Random();
            var questions = new List<Question>();

            foreach (var file in questionFiles)
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show($"Файл {file} не найден.");
                    continue;
                }

                var jsonContent = File.ReadAllText(file, Encoding.UTF8);
                try
                {
                    var loadedQuestions = JsonConvert.DeserializeObject<List<Question>>(jsonContent, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                    if (loadedQuestions != null)
                    {
                        questions.AddRange(loadedQuestions);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла {file}: {ex.Message}");
                }
            }

            var selectedQuestions = new List<Question>();
            while (selectedQuestions.Count < 10 && questions.Count > 0)
            {
                var index = random.Next(questions.Count);
                selectedQuestions.Add(questions[index]);
                questions.RemoveAt(index);
            }

            return selectedQuestions;
        }

        private void InitializeComponents()
        {
            this.Text = "Тестирование";
            this.Size = new System.Drawing.Size(1000, 800);

            this.Resize += FormTest_Resize;

            label1 = new Label();
            label1.Location = new System.Drawing.Point(20, 20);
            label1.Width = this.ClientSize.Width - 220;
            label1.Height = 60;

            panel1 = new Panel();
            panel1.Location = new System.Drawing.Point(20, 90);
            panel1.Width = this.ClientSize.Width - 220;
            panel1.Height = 200;
            panel1.BorderStyle = BorderStyle.FixedSingle;

            button1 = new Button();
            button1.Text = "Назад";
            button1.Location = new System.Drawing.Point(20, this.ClientSize.Height - 80);
            button1.Width = 100;
            button1.Height = 30;
            button1.BackColor = Color.LightGray;
            button1.Click += PrevButton_Click;

            button2 = new Button();
            button2.Text = "Далее";
            button2.Location = new System.Drawing.Point(150, this.ClientSize.Height - 80);
            button2.Width = 100;
            button2.Height = 30;
            button2.BackColor = Color.LightGray;
            button2.Click += NextButton_Click;

            button3 = new Button();
            button3.Text = "Завершить тест";
            button3.Location = new System.Drawing.Point(450, this.ClientSize.Height - 80);
            button3.Width = 150;
            button3.Height = 30;
            button3.BackColor = Color.LightGray;
            button3.Click += SubmitButton_Click;

            progressBar = new ProgressBar();
            progressBar.Location = new System.Drawing.Point(20, this.ClientSize.Height - 120);
            progressBar.Width = this.ClientSize.Width - 220;
            progressBar.Height = 30;
            progressBar.Maximum = 300; // 5 минут * 60 секунд = 300 секунд
            progressBar.Value = progressBar.Maximum;
            this.Controls.Add(progressBar);

            this.Controls.Add(label1);
            this.Controls.Add(panel1);
            this.Controls.Add(button1);
            this.Controls.Add(button2);
            this.Controls.Add(button3);

            InitializeQuestionButtons();
        }

        private void InitializeQuestionButtons()
        {
            questionButtons = new Button[10];
            for (int i = 0; i < 10; i++)
            {
                questionButtons[i] = new Button();
                questionButtons[i].Text = (i + 1).ToString();
                questionButtons[i].Location = new System.Drawing.Point(this.ClientSize.Width - 180, 20 + i * 30);
                questionButtons[i].Width = 50;
                questionButtons[i].Height = 30;
                questionButtons[i].Tag = i;
                questionButtons[i].Click += QuestionButton_Click;
                this.Controls.Add(questionButtons[i]);
            }
        }

        private void QuestionButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                currentQuestionIndex = (int)button.Tag;
                DisplayQuestion();
            }
        }

        private void DisplayQuestion()
        {
            if (currentQuestionIndex >= questions.Count)
            {
                MessageBox.Show("Индекс текущего вопроса находится вне допустимого диапазона.");
                return;
            }

            var question = questions[currentQuestionIndex];
            label1.Text = question.Text;
            panel1.Controls.Clear();

            if (question is SingleChoiceQuestion singleChoiceQuestion)
            {
                for (int i = 0; i < singleChoiceQuestion.Options.Length; i++)
                {
                    var radioButton = new RadioButton
                    {
                        Text = singleChoiceQuestion.Options[i],
                        Location = new System.Drawing.Point(20, i * 30),
                        Width = panel1.ClientSize.Width - 40,
                        Height = 30,
                        Tag = singleChoiceQuestion.Options[i]
                    };
                    radioButton.CheckedChanged += RadioButton_CheckedChanged;
                    panel1.Controls.Add(radioButton);

                    if (singleChoiceQuestion.SelectedAnswer == singleChoiceQuestion.Options[i])
                    {
                        radioButton.Checked = true;
                    }
                }
            }
            else if (question is MultipleChoiceQuestion multipleChoiceQuestion)
            {
                for (int i = 0; i < multipleChoiceQuestion.Options.Length; i++)
                {
                    var checkBox = new CheckBox
                    {
                        Text = multipleChoiceQuestion.Options[i],
                        Location = new System.Drawing.Point(20, i * 30),
                        Width = panel1.ClientSize.Width - 40,
                        Height = 30,
                        Tag = multipleChoiceQuestion.Options[i]
                    };
                    checkBox.CheckedChanged += CheckBox_CheckedChanged;
                    panel1.Controls.Add(checkBox);

                    if (multipleChoiceQuestion.SelectedAnswers.Contains(multipleChoiceQuestion.Options[i]))
                    {
                        checkBox.Checked = true;
                    }
                }
            }
            else if (question is TextQuestion textQuestion)
            {
                var textBox = new TextBox
                {
                    Location = new System.Drawing.Point(20, 20),
                    Width = panel1.ClientSize.Width - 40
                };
                textBox.TextChanged += (s, e) =>
                {
                    textQuestion.SelectedAnswer = textBox.Text;
                };
                textBox.Text = textQuestion.SelectedAnswer;
                panel1.Controls.Add(textBox);
            }
            else if (question is MatchingQuestion matchingQuestion)
            {
                var labels = matchingQuestion.Options.Select((option, index) => new Label
                {
                    Text = option.Split(new string[] { "->" }, StringSplitOptions.None)[0],
                    Location = new System.Drawing.Point(20,10 +  index * 30),
                    Width = panel1.ClientSize.Width / 2 - 40,
                    Height = 30
                }).ToList();
                var textBoxes = matchingQuestion.Options.Select((option, index) => new TextBox
                {
                    Location = new System.Drawing.Point(panel1.ClientSize.Width / 2, 10 + index * 30), //ответы на сплит вопросы
                    Width = panel1.ClientSize.Width / 2 - 40,
                    Height = 30,
                    Text = matchingQuestion.SelectedAnswer != null && matchingQuestion.SelectedAnswer.Length > index ? matchingQuestion.SelectedAnswer[index] : ""
                }).ToList();

                for (int i = 0; i < labels.Count; i++)
                {
                    panel1.Controls.Add(labels[i]);
                    panel1.Controls.Add(textBoxes[i]);
                    textBoxes[i].TextChanged += (s, e) =>
                    {
                        matchingQuestion.SelectedAnswer = textBoxes.Select(tb => tb.Text).ToArray();
                    };
                }
            }

            UpdateButtonVisibility();
            UpdateQuestionButtonColors();
        }

        private void UpdateButtonVisibility()
        {
            button1.Visible = currentQuestionIndex > 0;
            button2.Visible = currentQuestionIndex < questions.Count - 1;
            button3.Visible = currentQuestionIndex == questions.Count - 1;
        }
        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.Checked)
            {
                var question = questions[currentQuestionIndex] as SingleChoiceQuestion;
                if (question != null)
                {
                    question.SelectedAnswer = radioButton.Tag.ToString();
                }
                UpdateQuestionButtonColors();
            }
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                var question = questions[currentQuestionIndex] as MultipleChoiceQuestion;
                if (question != null)
                {
                    if (checkBox.Checked)
                    {
                        question.SelectedAnswers.Add(checkBox.Tag.ToString());
                    }
                    else
                    {
                        question.SelectedAnswers.Remove(checkBox.Tag.ToString());
                    }
                }
                UpdateQuestionButtonColors();
            }
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            if (currentQuestionIndex > 0)
            {
                currentQuestionIndex--;
                DisplayQuestion();
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (currentQuestionIndex < questions.Count - 1)
            {
                currentQuestionIndex++;
                DisplayQuestion();
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите завершить тест?", "Подтверждение", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                FormResults formResults = new FormResults(questions);
                formResults.ShowDialog(); // Используем ShowDialog для блокировки основной формы
                this.Close(); // Закрываем текущую форму теста после завершения
            }
        }

        private void FormTest_Resize(object sender, EventArgs e)
        {
            label1.Width = this.ClientSize.Width - 220;
            panel1.Width = this.ClientSize.Width - 220;
            button1.Location = new System.Drawing.Point(20, this.ClientSize.Height - 80);
            button2.Location = new System.Drawing.Point(150, this.ClientSize.Height - 80);
            button3.Location = new System.Drawing.Point(450, this.ClientSize.Height - 80);
            progressBar.Width = this.ClientSize.Width - 220;
            progressBar.Location = new System.Drawing.Point(20, this.ClientSize.Height - 120);
            for (int i = 0; i < 10; i++)
            {
                questionButtons[i].Location = new System.Drawing.Point(this.ClientSize.Width - 180, 20 + i * 30);
            }

            DisplayQuestion();
        }

        private void StartTimer()
        {
            timeLeft = 300; 
            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                timeLeft--;
                progressBar.Value = timeLeft;
            }
            else
            {
                timer.Stop();
                ShowResults();
            }
        }

        private void ShowResults()
        {
            FormResults formResults = new FormResults(questions);
            formResults.ShowDialog(); // Открываем форму с результатами как диалоговое окно
            this.Close(); // Закрываем текущую форму
        }
        private void UpdateQuestionButtonColors()
        {
            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                questionButtons[i].BackColor = question.IsAnswered() ? Color.LightGreen : Color.LightGray;
            }
        }
    }

    public class FormResults : Form
    {
        public FormResults(List<Question> questions)
        {
            this.Text = "Результаты теста";
            this.Size = new System.Drawing.Size(800, 600);

            var resultPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            int correctAnswers = 0;
            int yPosition = 20;

            foreach (var question in questions)
            {
                var questionLabel = new Label
                {
                    Text = question.Text,
                    Location = new Point(20, yPosition),
                    AutoSize = true
                };
                resultPanel.Controls.Add(questionLabel);

                var answerLabel = new Label
                {
                    Text = $"Ваш ответ: {question.GetSelectedAnswer()}",
                    Location = new Point(20, yPosition + 20),
                    AutoSize = true
                };
                answerLabel.ForeColor = question.IsCorrect() ? Color.Green : Color.Red;
                resultPanel.Controls.Add(answerLabel);

                if (!question.IsCorrect())
                {
                    var correctAnswerLabel = new Label
                    {
                        Text = $"Правильный ответ: {question.GetCorrectAnswer()}",
                        Location = new Point(20, yPosition + 40),
                        AutoSize = true,
                        ForeColor = Color.Blue
                    };
                    resultPanel.Controls.Add(correctAnswerLabel);
                    yPosition += 60;
                }
                else
                {
                    yPosition += 40;
                }

                if (question.IsCorrect())
                {
                    correctAnswers++;
                }
            }

            var summaryLabel = new Label
            {
                Text = $"Правильных ответов: {correctAnswers} из {questions.Count}",
                Location = new Point(20, yPosition + 20),
                AutoSize = true,
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            resultPanel.Controls.Add(summaryLabel);

            this.Controls.Add(resultPanel);
        }
    }

    public abstract class Question
    {
        public string Text { get; set; }
        public abstract bool IsCorrect();
        public abstract bool IsAnswered();
        public abstract string GetSelectedAnswer();
        public abstract string GetCorrectAnswer();
    }

    public class SingleChoiceQuestion : Question
    {
        public string[] Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string SelectedAnswer { get; set; }

        public override bool IsCorrect()
        {
            return SelectedAnswer == CorrectAnswer;
        }

        public override bool IsAnswered()
        {
            return !string.IsNullOrEmpty(SelectedAnswer);
        }

        public override string GetSelectedAnswer()
        {
            return SelectedAnswer;
        }

        public override string GetCorrectAnswer()
        {
            return CorrectAnswer;
        }
    }

    public class MultipleChoiceQuestion : Question
    {
        public string[] Options { get; set; }
        public string[] CorrectAnswers { get; set; }
        public HashSet<string> SelectedAnswers { get; set; } = new HashSet<string>();

        public override bool IsCorrect()
        {
            return SelectedAnswers.SetEquals(CorrectAnswers);
        }

        public override bool IsAnswered()
        {
            return SelectedAnswers.Count > 0;
        }

        public override string GetSelectedAnswer()
        {
            return string.Join(", ", SelectedAnswers);
        }

        public override string GetCorrectAnswer()
        {
            return string.Join(", ", CorrectAnswers);
        }
    }

    public class TextQuestion : Question
    {
        public string CorrectAnswer { get; set; }
        public string SelectedAnswer { get; set; }

        public override bool IsCorrect()
        {
            return SelectedAnswer?.Trim().ToLower() == CorrectAnswer.Trim().ToLower();
        }

        public override bool IsAnswered()
        {
            return !string.IsNullOrEmpty(SelectedAnswer);
        }

        public override string GetSelectedAnswer()
        {
            return SelectedAnswer;
        }

        public override string GetCorrectAnswer()
        {
            return CorrectAnswer;
        }
    }

    public class MatchingQuestion : Question
    {
        public string[] Options { get; set; }
        public string[] CorrectAnswer { get; set; }
        public string[] SelectedAnswer { get; set; } = new string[0];

        public override bool IsCorrect()
        {
            return SelectedAnswer.SequenceEqual(CorrectAnswer);
        }

        public override bool IsAnswered()
        {
            return SelectedAnswer != null && SelectedAnswer.Any(a => !string.IsNullOrEmpty(a));
        }

        public override string GetSelectedAnswer()
        {
            return string.Join(", ", SelectedAnswer);
        }

        public override string GetCorrectAnswer()
        {
            return string.Join(", ", CorrectAnswer);
        }
    }
}
