﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace TriNote
{
    public partial class FrmCadastro : Form
    {
        int idGerencia;
        int tipoPermissao;
        int qtdLoginIgual;
        Boolean valido = false;
        Boolean validoSenhaForte = false;
        Criptografia criptografia = new Criptografia();

        Conexao conexao;
        SqlDataAdapter dataAdapter;
        DataSet dataSet;

        public FrmCadastro(int id, int permissaoFuncionario)
        {
            InitializeComponent();
            idGerencia = id;
            tipoPermissao = 0;

            if (permissaoFuncionario == 3)
            {
                rbAdmin.Hide();
            }
        }

        private void btnCadastrar_Click(object sender, EventArgs e)
        {
            validarCampos();
            validarSenhaForte();
            string senhaFuncionario = txtSenha.Text;

            if (valido == true && validoSenhaForte == true)
            {
                conexao = new Conexao();
                conexao.conectar();
                criptografia = new Criptografia();
                senhaFuncionario = criptografia.criptografar(senhaFuncionario);
                conexao.command.CommandType = CommandType.StoredProcedure;
                conexao.command.CommandText = "usp_cadastrarFunc";
                //conexao.command.CommandText = "insert into Funcionario (nomeFuncionario,permissaoFuncionario,loginFuncionario,senhaFuncionario,statusFuncionario) values (@nomeFuncionario, @permissaoFuncionario, @loginFuncionario, @senhaFuncionario, 1)";
                conexao.command.Parameters.Add("@nomeFuncionario", SqlDbType.VarChar).Value = txtNome.Text;
                conexao.command.Parameters.Add("@permissaoFuncionario", SqlDbType.Int).Value = tipoPermissao;
                conexao.command.Parameters.Add("@loginFuncionario", SqlDbType.VarChar).Value = txtLogin.Text;
                conexao.command.Parameters.Add("@senhaFuncionario", SqlDbType.VarChar).Value = senhaFuncionario;
                conexao.command.ExecuteNonQuery();
                conexao.fechaConexao();

                MessageBox.Show("Cadastro do usuário " + txtNome.Text + " realizado!");

                this.Close();
            }
            else if (valido == false && validoSenhaForte == true && qtdLoginIgual == 0)
            {
                MessageBox.Show("Um ou mais campos estão em branco.");
            }
            else if (valido == false && validoSenhaForte == true && qtdLoginIgual > 0)
            {
                MessageBox.Show("Login existente.");
            }

        }

        private void validarSenhaForte()
        {
            // Padrão da senha: 7 caracteres, letras e número e 1 caracter especial no mínimo
            bool result = true;
            validoSenhaForte = true;
            string mensagemSenha = "Insira na senha no mínimo:";

            if (txtSenha.Text.Length < 7)
            {
                mensagemSenha += "\n7 caracteres.";
                validoSenhaForte = false;
            }

            result = Regex.IsMatch(txtSenha.Text, @"[a-zA-Z]");

            if (result == false)
            {
                mensagemSenha += "\nUma letra.";
                validoSenhaForte = false;
            }

            result = Regex.IsMatch(txtSenha.Text, @"[0-9]");

            if (result == false)
            {
                mensagemSenha += "\nUm número.";
                validoSenhaForte = false;
            }

            result = Regex.IsMatch(txtSenha.Text, @"[^\w\s]");

            if (result == false)
            {
                mensagemSenha += "\nUm caracter especial.";
                validoSenhaForte = false;
            }

            if(validoSenhaForte == false)
            {
                MessageBox.Show(mensagemSenha);
            }

        }

        private void validarCampos()
        {
            conexao = new Conexao();
            conexao.conectar();
            conexao.command.CommandText = "select count(loginfuncionario) as qtdLogin from funcionario where loginFuncionario=@loginFuncionario";
            conexao.command.Parameters.Add("@loginFuncionario", SqlDbType.VarChar).Value = txtLogin.Text;
            conexao.command.ExecuteNonQuery();

            dataAdapter = new SqlDataAdapter();
            dataSet = new DataSet();
            dataAdapter.SelectCommand = conexao.command;
            dataAdapter.Fill(dataSet);
            conexao.fechaConexao();

            qtdLoginIgual = Convert.ToInt32(dataSet.Tables[0].Rows[0]["qtdLogin"]);

            if (txtNome.Text == "" || string.IsNullOrWhiteSpace(txtNome.Text))
            {
                valido = false;
            }
            else if (txtLogin.Text == "" || string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                valido = false;
            }
            else if (txtSenha.Text == "" || string.IsNullOrWhiteSpace(txtSenha.Text) || qtdLoginIgual > 0)
            {
                valido = false;
                //MessageBox.Show("login igual no banco");
            }
            else if (txtLogin.Text == txtSenha.Text) // Login não pode ser igual a senha
            {
                valido = false;
            }
            else if (txtConfirmarSenha.Text == "" || string.IsNullOrWhiteSpace(txtConfirmarSenha.Text))
            {
                valido = false;
            }
            else if (txtSenha.Text != txtConfirmarSenha.Text)
            {
                valido = false;
            }
            else if (!rbSuporte.Enabled || !rbMarketing.Enabled || !rbGerencia.Enabled || !rbAdmin.Enabled)
            {
                valido = false;
            }
            else if (tipoPermissao == 0)
            {
                valido = false;
            }            
            else
            {
                valido = true;
            }

        }
        
        private void rbSuporte_Click(object sender, EventArgs e)
        {
            tipoPermissao = 1;
        }

        private void rbMarketing_Click(object sender, EventArgs e)
        {
            tipoPermissao = 2;
        }

        private void rbGerencia_Click(object sender, EventArgs e)
        {
            tipoPermissao = 3;
        }

        private void rbAdmin_Click(object sender, EventArgs e)
        {
            tipoPermissao = 4;
        }
    }
}
