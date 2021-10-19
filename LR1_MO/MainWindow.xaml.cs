using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LR1_MO {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }
        public void Button_Click(object sender, RoutedEventArgs e) {
            //Инициализация СЛАУ
            string z = textBox1.Text;
            int m = int.Parse(textBox1.Text);
            int n = int.Parse(textBox2.Text);
            double[,] A = new double[m, n];               //Матрица А
            double[,] B = new double[m, 1];               //Матрица B

            //Заполнение матрицы А
            string s = textBox3.Text;
            char[] s2 = { ' ', '\n', ',', ';', '\r', '\t'};
            string[] s1 = s.Split(s2);

            int k = 0;
            for (int i = 0; i < m; i++) {
                for (int j = 0; j < n; j++) {
                    if (s1[k] == "") k++;
                    A[i, j] = double.Parse(s1[k]);
                    k++;
                }
            }

            //Заполнение матрицы В
            string ss = textBox4.Text;
            string[] ss1 = ss.Split(s2);
            int kk = 0;
            for (int i = 0; i < m; i++) {
                if (ss1[kk] == "") kk++;
                B[i, 0] = double.Parse(ss1[kk]);
                kk++;
            }

            //Поиск псевдообратной матрицы
            double[,] A_plus = Faddeev_algorithm(m, n, A);
            //Нахождение решения уравнения
            double[,] X = Mult_matrix(n, m, m, 1, A_plus, B);

            //---------------------------------------------------------

            //Вывод ответа
            textBox10.Text = "Решение уравнения AX=B: \r\nX: \r\n(  ";
            for (int i = 0; i < n; i++) {
                textBox10.Text += Math.Round(X[i,0], 6).ToString() + "  ";
            }
            textBox10.Text += ")";

            double[,] b_num = Mult_matrix(m, n, n, 1, A, X);
            double[,] b_discrepancy = Subtruct_matrix(m, 1, B, b_num);
            double diz_rate = 0.0;
            for (int i = 0; i < m; i++) {
                diz_rate += Math.Pow(b_discrepancy[i,0], 2);
            }
            textBox10.Text += "\r\n\r\nНорма невязки: " + Math.Round(Math.Sqrt(diz_rate), 6).ToString();
        }

        static double[,] Faddeev_algorithm(int m, int n, double[,] A){
            //Открытие файла на запись
            FileStream file = new FileStream(
                "C:\\Users\\Медведев Владислав\\Desktop\\LR1_MO\\Промежуточные вычисления.txt",
                 FileMode.Create);
            StreamWriter ww = new StreamWriter(file);

            double[,] I = new double[n, n];     //Единичная матрица

            //Заполнение единичной матрицы
            for (int i = 0; i < n; i++){
                for (int j = 0; j < n; j++){
                    I[i, j] = 0;
                    if (i == j) I[i, j] = 1;
                }
            }

            //Вспомогательные вычисления:
            double[,] A_T = Transpose(m, n, A);
            double[,] A_T_A = Mult_matrix(n, m, m, n, A_T, A);

            //Шаг 1.
            ww.WriteLine("Промежуточные расчёты нахождения псевдообратной матрицы:");
            ww.WriteLine("Шаг 1.");

            double[,] Phi = I,
                      Phi_ = I;
            double phi = Add_main_diag(A_T_A, n),
                   phi_ = Add_main_diag(A_T_A, n);

            ww.WriteLine("phi = " + phi.ToString());
            ww.WriteLine("F:");
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < n; j++) {
                    ww.Write(Phi[i, j].ToString() + "\t");
                }
                ww.WriteLine();
            }
            double num = 2.0;

            //Шаг 2 и следующие.
            while (phi != 0){
                //double[,] pop = Mult_matrix(n, n, n, n, A_T_A, Phi);
                //double[,] dod = Subtruct_matrix(n, n, Scalar_mult(n, n, phi, I), Mult_matrix(n, n, n, n, A_T_A, Phi));
                double buf = (1.0 / num) * Add_main_diag(Mult_matrix(n, n, n, n, A_T_A, Subtruct_matrix(n, n, Scalar_mult(n, n, phi, I), Mult_matrix(n, n, n, n, A_T_A, Phi))), n);
                if (buf == 0) {
                    //Шаг l.
                    phi_ = phi;
                    Phi_ = Phi;
                }

                Phi = Subtruct_matrix(n, n, Scalar_mult(n, n, phi, I), Mult_matrix(n, n, n, n, A_T_A, Phi));
                phi = (1.0 / num) * Add_main_diag(Mult_matrix(n, n, n, n, A_T_A, Phi), n);

                if (buf != 0) {
                    ww.WriteLine("\nШаг " + num.ToString() + ".");
                    ww.WriteLine("phi = " + phi.ToString());
                    ww.WriteLine("F: ");
                    for (int i = 0; i < n; i++){
                        for (int j = 0; j < n; j++){
                            ww.Write(Phi[i, j].ToString() + "\t");
                        }
                        ww.WriteLine();
                    }
                }

                num++;
            }

            double[,] A_plus = Mult_matrix(n, n, n, m, Scalar_mult(n, n, (1.0 / phi_), Phi_), A_T);

            ww.WriteLine("\nПсевдообратная матрица:");
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < m; j++) {
                    ww.Write(Math.Round(A_plus[i, j], 6).ToString() + "\t");
                }
                ww.WriteLine();
            }

            ww.Close();
            return A_plus;
        }

        static double[,] Mult_matrix(int m1, int n1, int m2, int n2, double[,] A1, double[,] A2) {
            double[,] C_mult = new double[m1, n2];

            for (int i = 0; i < m1; i++) {
                for (int j = 0; j < n2; j++) {
                    for (int k = 0; k < m2; k++) {
                        C_mult[i, j] += A1[i, k] * A2[k, j];
                    }
                }
            }
            return C_mult;
        }

        static double[,] Transpose(int m, int n, double[,] A) {
            double[,] A_trans = new double[n, m];

            for (int i = 0; i < n; i++) {
                for (int j = 0; j < m; j++) {
                    A_trans[i, j] = A[j, i];
                }
            }

            return A_trans;
        }

        static double Add_main_diag(double[,] A_, int n) {
            double sum = 0;
            for (int i = 0; i < n; i++) {
                sum += A_[i, i];
            }
            return sum;
        }

        static double[,] Scalar_mult(int m, int n, double phi, double[,] M) {
            double[,] Rez = new double[m, n];
            for (int i = 0; i < m; i++) {
                for (int j = 0; j < n; j++) {
                    Rez[i, j] = phi * M[i, j];
                }
            }
            return Rez;
        }

        static double[,] Subtruct_matrix(int m, int n, double[,] A, double[,] B) {
            double[,] C = new double[m, n];
            for (int i = 0; i < m; i++) {
                for (int j = 0; j < n; j++) {
                    C[i, j] = A[i, j] - B[i, j];
                }
            }
            return C;
        }
    }
}
