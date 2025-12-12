using System.Drawing;
using System.Windows.Forms;

namespace ModuleGPI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.treeFavoritos = new System.Windows.Forms.TreeView();
            this.txtFiltroCategoria = new System.Windows.Forms.TextBox();
            this.lblFavoritos = new System.Windows.Forms.Label();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabDashboard = new System.Windows.Forms.TabPage();
            this.tlpDash = new System.Windows.Forms.TableLayoutPanel();
            this.flpKPIs = new System.Windows.Forms.FlowLayoutPanel();
            this.dgvLaunchLog = new System.Windows.Forms.DataGridView();
            this.tabModulos = new System.Windows.Forms.TabPage();
            this.flpModulos = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlModulosHeader = new System.Windows.Forms.Panel();
            this.lblModulosTitulo = new System.Windows.Forms.Label();
            this.txtModulosSearch = new System.Windows.Forms.TextBox();
            this.btnModulosRefrescar = new System.Windows.Forms.Button();
            this.tabModulosTest = new System.Windows.Forms.TabPage();
            this.flpModulosTest = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlModulosTestHeader = new System.Windows.Forms.Panel();
            this.lblModulosTestTitulo = new System.Windows.Forms.Label();
            this.txtModulosTestSearch = new System.Windows.Forms.TextBox();
            this.btnModulosTestRefrescar = new System.Windows.Forms.Button();
            this.tabAdmin = new System.Windows.Forms.TabPage();
            this.pnlAdminButtons = new System.Windows.Forms.Panel();
            this.btnAdminGuardar = new System.Windows.Forms.Button();
            this.btnAdminRefrescar = new System.Windows.Forms.Button();
            this.splitAdmin = new System.Windows.Forms.SplitContainer();
            this.leftAdmin = new System.Windows.Forms.Panel();
            this.grpRoles = new System.Windows.Forms.GroupBox();
            this.barFiltroPlanta = new System.Windows.Forms.FlowLayoutPanel();
            this.chkPlantFilter = new System.Windows.Forms.CheckBox();
            this.cboPlantFilter = new System.Windows.Forms.ComboBox();
            this.dgvUsuarios = new System.Windows.Forms.DataGridView();
            this.rightAdmin = new System.Windows.Forms.Panel();
            this.dgvModulos = new System.Windows.Forms.DataGridView();
            this.tabConfig = new System.Windows.Forms.TabPage();
            this.btnDeleteModule = new System.Windows.Forms.Button();
            this.btnSaveModule = new System.Windows.Forms.Button();
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.dgvModulesConfig = new System.Windows.Forms.DataGridView();
            this.grpAcerca = new System.Windows.Forms.GroupBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnNewModule = new System.Windows.Forms.Button();
            this.cmuModulo = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmuAbrir = new System.Windows.Forms.ToolStripMenuItem();
            this.cmuAbrirAdmin = new System.Windows.Forms.ToolStripMenuItem();
            this.cmuCopiarRuta = new System.Windows.Forms.ToolStripMenuItem();
            this.cmuVerProp = new System.Windows.Forms.ToolStripMenuItem();
            this.statusMain = new System.Windows.Forms.StatusStrip();
            this.tsslUser = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslRole = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslPlant = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslSpring = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslEstado = new System.Windows.Forms.ToolStripStatusLabel();
            this.root = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTopBar = new System.Windows.Forms.Panel();
            this.btnCerrarSesion = new System.Windows.Forms.Button();
            this.lblTitulo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabDashboard.SuspendLayout();
            this.tlpDash.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLaunchLog)).BeginInit();
            this.tabModulos.SuspendLayout();
            this.pnlModulosHeader.SuspendLayout();
            this.tabModulosTest.SuspendLayout();
            this.pnlModulosTestHeader.SuspendLayout();
            this.tabAdmin.SuspendLayout();
            this.pnlAdminButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitAdmin)).BeginInit();
            this.splitAdmin.Panel1.SuspendLayout();
            this.splitAdmin.Panel2.SuspendLayout();
            this.splitAdmin.SuspendLayout();
            this.leftAdmin.SuspendLayout();
            this.grpRoles.SuspendLayout();
            this.barFiltroPlanta.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).BeginInit();
            this.rightAdmin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvModulos)).BeginInit();
            this.tabConfig.SuspendLayout();
            this.grpGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvModulesConfig)).BeginInit();
            this.grpAcerca.SuspendLayout();
            this.cmuModulo.SuspendLayout();
            this.statusMain.SuspendLayout();
            this.root.SuspendLayout();
            this.pnlTopBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.Location = new System.Drawing.Point(3, 53);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.pnlLeft);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.tabMain);
            this.splitMain.Size = new System.Drawing.Size(1002, 523);
            this.splitMain.SplitterDistance = 150;
            this.splitMain.TabIndex = 2;
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.treeFavoritos);
            this.pnlLeft.Controls.Add(this.txtFiltroCategoria);
            this.pnlLeft.Controls.Add(this.lblFavoritos);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(150, 523);
            this.pnlLeft.TabIndex = 0;
            // 
            // treeFavoritos
            // 
            this.treeFavoritos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeFavoritos.Location = new System.Drawing.Point(0, 44);
            this.treeFavoritos.Name = "treeFavoritos";
            this.treeFavoritos.Size = new System.Drawing.Size(150, 479);
            this.treeFavoritos.TabIndex = 0;
            // 
            // txtFiltroCategoria
            // 
            this.txtFiltroCategoria.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtFiltroCategoria.Location = new System.Drawing.Point(0, 24);
            this.txtFiltroCategoria.Name = "txtFiltroCategoria";
            this.txtFiltroCategoria.Size = new System.Drawing.Size(150, 20);
            this.txtFiltroCategoria.TabIndex = 1;
            this.txtFiltroCategoria.Tag = "Filtrar categorías…";
            // 
            // lblFavoritos
            // 
            this.lblFavoritos.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFavoritos.Location = new System.Drawing.Point(0, 0);
            this.lblFavoritos.Name = "lblFavoritos";
            this.lblFavoritos.Padding = new System.Windows.Forms.Padding(6, 6, 0, 0);
            this.lblFavoritos.Size = new System.Drawing.Size(150, 24);
            this.lblFavoritos.TabIndex = 2;
            this.lblFavoritos.Text = "Favoritos";
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabDashboard);
            this.tabMain.Controls.Add(this.tabModulos);
            this.tabMain.Controls.Add(this.tabModulosTest);
            this.tabMain.Controls.Add(this.tabAdmin);
            this.tabMain.Controls.Add(this.tabConfig);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(848, 523);
            this.tabMain.TabIndex = 0;
            // 
            // tabDashboard
            // 
            this.tabDashboard.Controls.Add(this.tlpDash);
            this.tabDashboard.Location = new System.Drawing.Point(4, 22);
            this.tabDashboard.Name = "tabDashboard";
            this.tabDashboard.Size = new System.Drawing.Size(840, 497);
            this.tabDashboard.TabIndex = 0;
            this.tabDashboard.Text = "Dashboard";
            // 
            // tlpDash
            // 
            this.tlpDash.ColumnCount = 1;
            this.tlpDash.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDash.Controls.Add(this.flpKPIs, 0, 0);
            this.tlpDash.Controls.Add(this.dgvLaunchLog, 0, 1);
            this.tlpDash.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDash.Location = new System.Drawing.Point(0, 0);
            this.tlpDash.Name = "tlpDash";
            this.tlpDash.RowCount = 2;
            this.tlpDash.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpDash.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDash.Size = new System.Drawing.Size(840, 497);
            this.tlpDash.TabIndex = 0;
            // 
            // flpKPIs
            // 
            this.flpKPIs.AutoScroll = true;
            this.flpKPIs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpKPIs.Location = new System.Drawing.Point(3, 3);
            this.flpKPIs.Name = "flpKPIs";
            this.flpKPIs.Padding = new System.Windows.Forms.Padding(8);
            this.flpKPIs.Size = new System.Drawing.Size(834, 114);
            this.flpKPIs.TabIndex = 0;
            // 
            // dgvLaunchLog
            // 
            this.dgvLaunchLog.AllowUserToAddRows = false;
            this.dgvLaunchLog.AllowUserToDeleteRows = false;
            this.dgvLaunchLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLaunchLog.Location = new System.Drawing.Point(3, 123);
            this.dgvLaunchLog.Name = "dgvLaunchLog";
            this.dgvLaunchLog.ReadOnly = true;
            this.dgvLaunchLog.Size = new System.Drawing.Size(834, 371);
            this.dgvLaunchLog.TabIndex = 1;
            // 
            // tabModulos
            // 
            this.tabModulos.Controls.Add(this.flpModulos);
            this.tabModulos.Controls.Add(this.pnlModulosHeader);
            this.tabModulos.Location = new System.Drawing.Point(4, 22);
            this.tabModulos.Name = "tabModulos";
            this.tabModulos.Size = new System.Drawing.Size(840, 497);
            this.tabModulos.TabIndex = 1;
            this.tabModulos.Text = "Módulos GPI";
            // 
            // flpModulos
            // 
            this.flpModulos.AutoScroll = true;
            this.flpModulos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpModulos.Location = new System.Drawing.Point(0, 48);
            this.flpModulos.Name = "flpModulos";
            this.flpModulos.Padding = new System.Windows.Forms.Padding(8);
            this.flpModulos.Size = new System.Drawing.Size(840, 449);
            this.flpModulos.TabIndex = 0;
            // 
            // pnlModulosHeader
            // 
            this.pnlModulosHeader.Controls.Add(this.lblModulosTitulo);
            this.pnlModulosHeader.Controls.Add(this.txtModulosSearch);
            this.pnlModulosHeader.Controls.Add(this.btnModulosRefrescar);
            this.pnlModulosHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlModulosHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlModulosHeader.Name = "pnlModulosHeader";
            this.pnlModulosHeader.Padding = new System.Windows.Forms.Padding(8);
            this.pnlModulosHeader.Size = new System.Drawing.Size(840, 48);
            this.pnlModulosHeader.TabIndex = 1;
            // 
            // lblModulosTitulo
            // 
            this.lblModulosTitulo.AutoSize = true;
            this.lblModulosTitulo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblModulosTitulo.Location = new System.Drawing.Point(8, 14);
            this.lblModulosTitulo.Name = "lblModulosTitulo";
            this.lblModulosTitulo.Size = new System.Drawing.Size(94, 19);
            this.lblModulosTitulo.TabIndex = 0;
            this.lblModulosTitulo.Text = "Módulos GPI";
            // 
            // txtModulosSearch
            // 
            this.txtModulosSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtModulosSearch.Location = new System.Drawing.Point(498, 12);
            this.txtModulosSearch.Name = "txtModulosSearch";
            this.txtModulosSearch.Size = new System.Drawing.Size(220, 20);
            this.txtModulosSearch.TabIndex = 1;
            // 
            // btnModulosRefrescar
            // 
            this.btnModulosRefrescar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnModulosRefrescar.Location = new System.Drawing.Point(726, 11);
            this.btnModulosRefrescar.Name = "btnModulosRefrescar";
            this.btnModulosRefrescar.Size = new System.Drawing.Size(100, 23);
            this.btnModulosRefrescar.TabIndex = 2;
            this.btnModulosRefrescar.Text = "Refrescar";
            // 
            // tabModulosTest
            // 
            this.tabModulosTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(250)))), ((int)(((byte)(240)))));
            this.tabModulosTest.Controls.Add(this.flpModulosTest);
            this.tabModulosTest.Controls.Add(this.pnlModulosTestHeader);
            this.tabModulosTest.Location = new System.Drawing.Point(4, 22);
            this.tabModulosTest.Name = "tabModulosTest";
            this.tabModulosTest.Size = new System.Drawing.Size(840, 497);
            this.tabModulosTest.TabIndex = 2;
            this.tabModulosTest.Text = "Módulos TEST";
            // 
            // flpModulosTest
            // 
            this.flpModulosTest.AutoScroll = true;
            this.flpModulosTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(250)))), ((int)(((byte)(240)))));
            this.flpModulosTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpModulosTest.Location = new System.Drawing.Point(0, 48);
            this.flpModulosTest.Name = "flpModulosTest";
            this.flpModulosTest.Padding = new System.Windows.Forms.Padding(8);
            this.flpModulosTest.Size = new System.Drawing.Size(840, 449);
            this.flpModulosTest.TabIndex = 0;
            // 
            // pnlModulosTestHeader
            // 
            this.pnlModulosTestHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(243)))), ((int)(((byte)(205)))));
            this.pnlModulosTestHeader.Controls.Add(this.lblModulosTestTitulo);
            this.pnlModulosTestHeader.Controls.Add(this.txtModulosTestSearch);
            this.pnlModulosTestHeader.Controls.Add(this.btnModulosTestRefrescar);
            this.pnlModulosTestHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlModulosTestHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlModulosTestHeader.Name = "pnlModulosTestHeader";
            this.pnlModulosTestHeader.Padding = new System.Windows.Forms.Padding(8);
            this.pnlModulosTestHeader.Size = new System.Drawing.Size(840, 48);
            this.pnlModulosTestHeader.TabIndex = 1;
            // 
            // lblModulosTestTitulo
            // 
            this.lblModulosTestTitulo.AutoSize = true;
            this.lblModulosTestTitulo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblModulosTestTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
            this.lblModulosTestTitulo.Location = new System.Drawing.Point(8, 14);
            this.lblModulosTestTitulo.Name = "lblModulosTestTitulo";
            this.lblModulosTestTitulo.Size = new System.Drawing.Size(102, 19);
            this.lblModulosTestTitulo.TabIndex = 0;
            this.lblModulosTestTitulo.Text = "Módulos TEST";
            // 
            // txtModulosTestSearch
            // 
            this.txtModulosTestSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtModulosTestSearch.Location = new System.Drawing.Point(498, 12);
            this.txtModulosTestSearch.Name = "txtModulosTestSearch";
            this.txtModulosTestSearch.Size = new System.Drawing.Size(220, 20);
            this.txtModulosTestSearch.TabIndex = 1;
            // 
            // btnModulosTestRefrescar
            // 
            this.btnModulosTestRefrescar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnModulosTestRefrescar.Location = new System.Drawing.Point(726, 11);
            this.btnModulosTestRefrescar.Name = "btnModulosTestRefrescar";
            this.btnModulosTestRefrescar.Size = new System.Drawing.Size(100, 23);
            this.btnModulosTestRefrescar.TabIndex = 2;
            this.btnModulosTestRefrescar.Text = "Refrescar";
            this.btnModulosTestRefrescar.UseVisualStyleBackColor = true;
            // 
            // tabAdmin
            // 
            this.tabAdmin.Controls.Add(this.pnlAdminButtons);
            this.tabAdmin.Controls.Add(this.splitAdmin);
            this.tabAdmin.Location = new System.Drawing.Point(4, 22);
            this.tabAdmin.Name = "tabAdmin";
            this.tabAdmin.Size = new System.Drawing.Size(840, 497);
            this.tabAdmin.TabIndex = 3;
            this.tabAdmin.Text = "Administración";
            // 
            // pnlAdminButtons
            // 
            this.pnlAdminButtons.Controls.Add(this.btnAdminGuardar);
            this.pnlAdminButtons.Controls.Add(this.btnAdminRefrescar);
            this.pnlAdminButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlAdminButtons.Location = new System.Drawing.Point(0, 453);
            this.pnlAdminButtons.Name = "pnlAdminButtons";
            this.pnlAdminButtons.Padding = new System.Windows.Forms.Padding(8);
            this.pnlAdminButtons.Size = new System.Drawing.Size(840, 44);
            this.pnlAdminButtons.TabIndex = 1;
            // 
            // btnAdminGuardar
            // 
            this.btnAdminGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdminGuardar.Location = new System.Drawing.Point(732, 8);
            this.btnAdminGuardar.Name = "btnAdminGuardar";
            this.btnAdminGuardar.Size = new System.Drawing.Size(100, 28);
            this.btnAdminGuardar.TabIndex = 0;
            this.btnAdminGuardar.Text = "Guardar";
            // 
            // btnAdminRefrescar
            // 
            this.btnAdminRefrescar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdminRefrescar.Location = new System.Drawing.Point(624, 8);
            this.btnAdminRefrescar.Name = "btnAdminRefrescar";
            this.btnAdminRefrescar.Size = new System.Drawing.Size(100, 28);
            this.btnAdminRefrescar.TabIndex = 1;
            this.btnAdminRefrescar.Text = "Refrescar";
            // 
            // splitAdmin
            // 
            this.splitAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitAdmin.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitAdmin.Location = new System.Drawing.Point(0, 0);
            this.splitAdmin.Name = "splitAdmin";
            // 
            // splitAdmin.Panel1
            // 
            this.splitAdmin.Panel1.Controls.Add(this.leftAdmin);
            // 
            // splitAdmin.Panel2
            // 
            this.splitAdmin.Panel2.Controls.Add(this.rightAdmin);
            this.splitAdmin.Panel2MinSize = 260;
            this.splitAdmin.Size = new System.Drawing.Size(840, 497);
            this.splitAdmin.SplitterDistance = 561;
            this.splitAdmin.SplitterWidth = 5;
            this.splitAdmin.TabIndex = 0;
            // 
            // leftAdmin
            // 
            this.leftAdmin.Controls.Add(this.grpRoles);
            this.leftAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftAdmin.Location = new System.Drawing.Point(0, 0);
            this.leftAdmin.Name = "leftAdmin";
            this.leftAdmin.Size = new System.Drawing.Size(561, 497);
            this.leftAdmin.TabIndex = 0;
            // 
            // grpRoles
            // 
            this.grpRoles.Controls.Add(this.barFiltroPlanta);
            this.grpRoles.Controls.Add(this.dgvUsuarios);
            this.grpRoles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpRoles.Location = new System.Drawing.Point(0, 0);
            this.grpRoles.Name = "grpRoles";
            this.grpRoles.Padding = new System.Windows.Forms.Padding(8);
            this.grpRoles.Size = new System.Drawing.Size(561, 497);
            this.grpRoles.TabIndex = 1;
            this.grpRoles.TabStop = false;
            this.grpRoles.Text = "Roles y Usuarios";
            // 
            // barFiltroPlanta
            // 
            this.barFiltroPlanta.Controls.Add(this.chkPlantFilter);
            this.barFiltroPlanta.Controls.Add(this.cboPlantFilter);
            this.barFiltroPlanta.Dock = System.Windows.Forms.DockStyle.Top;
            this.barFiltroPlanta.Location = new System.Drawing.Point(8, 21);
            this.barFiltroPlanta.Margin = new System.Windows.Forms.Padding(0);
            this.barFiltroPlanta.Name = "barFiltroPlanta";
            this.barFiltroPlanta.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.barFiltroPlanta.Size = new System.Drawing.Size(545, 40);
            this.barFiltroPlanta.TabIndex = 0;
            this.barFiltroPlanta.WrapContents = false;
            // 
            // chkPlantFilter
            // 
            this.chkPlantFilter.AutoSize = true;
            this.chkPlantFilter.Location = new System.Drawing.Point(6, 13);
            this.chkPlantFilter.Margin = new System.Windows.Forms.Padding(0, 8, 12, 0);
            this.chkPlantFilter.Name = "chkPlantFilter";
            this.chkPlantFilter.Size = new System.Drawing.Size(101, 17);
            this.chkPlantFilter.TabIndex = 0;
            this.chkPlantFilter.Text = "Filtrar por planta";
            // 
            // cboPlantFilter
            // 
            this.cboPlantFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPlantFilter.Location = new System.Drawing.Point(119, 10);
            this.cboPlantFilter.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.cboPlantFilter.Name = "cboPlantFilter";
            this.cboPlantFilter.Size = new System.Drawing.Size(160, 21);
            this.cboPlantFilter.TabIndex = 1;
            // 
            // dgvUsuarios
            // 
            this.dgvUsuarios.AllowUserToAddRows = false;
            this.dgvUsuarios.AllowUserToDeleteRows = false;
            this.dgvUsuarios.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.dgvUsuarios.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvUsuarios.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsuarios.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvUsuarios.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvUsuarios.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUsuarios.Location = new System.Drawing.Point(8, 21);
            this.dgvUsuarios.MultiSelect = false;
            this.dgvUsuarios.Name = "dgvUsuarios";
            this.dgvUsuarios.RowHeadersVisible = false;
            this.dgvUsuarios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsuarios.Size = new System.Drawing.Size(545, 468);
            this.dgvUsuarios.TabIndex = 1;
            // 
            // rightAdmin
            // 
            this.rightAdmin.Controls.Add(this.dgvModulos);
            this.rightAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightAdmin.Location = new System.Drawing.Point(0, 0);
            this.rightAdmin.Name = "rightAdmin";
            this.rightAdmin.Size = new System.Drawing.Size(274, 497);
            this.rightAdmin.TabIndex = 0;
            // 
            // dgvModulos
            // 
            this.dgvModulos.AllowUserToAddRows = false;
            this.dgvModulos.AllowUserToDeleteRows = false;
            this.dgvModulos.AllowUserToResizeRows = false;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.dgvModulos.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvModulos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvModulos.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvModulos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvModulos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvModulos.Location = new System.Drawing.Point(0, 0);
            this.dgvModulos.MultiSelect = false;
            this.dgvModulos.Name = "dgvModulos";
            this.dgvModulos.ReadOnly = true;
            this.dgvModulos.RowHeadersVisible = false;
            this.dgvModulos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvModulos.Size = new System.Drawing.Size(274, 497);
            this.dgvModulos.TabIndex = 0;
            // 
            // tabConfig
            // 
            this.tabConfig.Controls.Add(this.btnDeleteModule);
            this.tabConfig.Controls.Add(this.btnSaveModule);
            this.tabConfig.Controls.Add(this.grpGeneral);
            this.tabConfig.Controls.Add(this.grpAcerca);
            this.tabConfig.Controls.Add(this.btnNewModule);
            this.tabConfig.Location = new System.Drawing.Point(4, 22);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.Size = new System.Drawing.Size(840, 497);
            this.tabConfig.TabIndex = 4;
            this.tabConfig.Text = "Configuración";
            // 
            // btnDeleteModule
            // 
            this.btnDeleteModule.Location = new System.Drawing.Point(262, 262);
            this.btnDeleteModule.Name = "btnDeleteModule";
            this.btnDeleteModule.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteModule.TabIndex = 4;
            this.btnDeleteModule.Text = "Eliminar!";
            this.btnDeleteModule.UseVisualStyleBackColor = true;
            // 
            // btnSaveModule
            // 
            this.btnSaveModule.Location = new System.Drawing.Point(156, 262);
            this.btnSaveModule.Name = "btnSaveModule";
            this.btnSaveModule.Size = new System.Drawing.Size(100, 30);
            this.btnSaveModule.TabIndex = 3;
            this.btnSaveModule.Text = "Editar Modulo";
            this.btnSaveModule.UseVisualStyleBackColor = true;
            // 
            // grpGeneral
            // 
            this.grpGeneral.Controls.Add(this.dgvModulesConfig);
            this.grpGeneral.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpGeneral.Location = new System.Drawing.Point(0, 0);
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Padding = new System.Windows.Forms.Padding(8);
            this.grpGeneral.Size = new System.Drawing.Size(840, 256);
            this.grpGeneral.TabIndex = 0;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "General";
            // 
            // dgvModulesConfig
            // 
            this.dgvModulesConfig.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvModulesConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvModulesConfig.Location = new System.Drawing.Point(8, 21);
            this.dgvModulesConfig.Name = "dgvModulesConfig";
            this.dgvModulesConfig.Size = new System.Drawing.Size(824, 227);
            this.dgvModulesConfig.TabIndex = 1;
            // 
            // grpAcerca
            // 
            this.grpAcerca.Controls.Add(this.lblVersion);
            this.grpAcerca.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grpAcerca.Location = new System.Drawing.Point(0, 377);
            this.grpAcerca.Name = "grpAcerca";
            this.grpAcerca.Padding = new System.Windows.Forms.Padding(8);
            this.grpAcerca.Size = new System.Drawing.Size(840, 120);
            this.grpAcerca.TabIndex = 1;
            this.grpAcerca.TabStop = false;
            this.grpAcerca.Text = "Acerca de";
            // 
            // lblVersion
            // 
            this.lblVersion.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblVersion.Location = new System.Drawing.Point(8, 21);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(824, 21);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "Versión: 2.0.0";
            // 
            // btnNewModule
            // 
            this.btnNewModule.Location = new System.Drawing.Point(50, 262);
            this.btnNewModule.Name = "btnNewModule";
            this.btnNewModule.Size = new System.Drawing.Size(100, 30);
            this.btnNewModule.TabIndex = 2;
            this.btnNewModule.Text = "Nuevo Modulo";
            this.btnNewModule.UseVisualStyleBackColor = true;
            // 
            // cmuModulo
            // 
            this.cmuModulo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmuAbrir,
            this.cmuAbrirAdmin,
            this.cmuCopiarRuta,
            this.cmuVerProp});
            this.cmuModulo.Name = "cmuModulo";
            this.cmuModulo.Size = new System.Drawing.Size(172, 92);
            // 
            // cmuAbrir
            // 
            this.cmuAbrir.Name = "cmuAbrir";
            this.cmuAbrir.Size = new System.Drawing.Size(171, 22);
            this.cmuAbrir.Text = "Abrir";
            // 
            // cmuAbrirAdmin
            // 
            this.cmuAbrirAdmin.Name = "cmuAbrirAdmin";
            this.cmuAbrirAdmin.Size = new System.Drawing.Size(171, 22);
            this.cmuAbrirAdmin.Text = "Abrir como admin";
            // 
            // cmuCopiarRuta
            // 
            this.cmuCopiarRuta.Name = "cmuCopiarRuta";
            this.cmuCopiarRuta.Size = new System.Drawing.Size(171, 22);
            this.cmuCopiarRuta.Text = "Copiar ruta";
            // 
            // cmuVerProp
            // 
            this.cmuVerProp.Name = "cmuVerProp";
            this.cmuVerProp.Size = new System.Drawing.Size(171, 22);
            this.cmuVerProp.Text = "Propiedades";
            // 
            // statusMain
            // 
            this.statusMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslUser,
            this.tsslRole,
            this.tsslPlant,
            this.tsslSpring,
            this.tsslEstado});
            this.statusMain.Location = new System.Drawing.Point(0, 579);
            this.statusMain.Name = "statusMain";
            this.statusMain.Size = new System.Drawing.Size(1008, 22);
            this.statusMain.TabIndex = 3;
            // 
            // tsslUser
            // 
            this.tsslUser.Name = "tsslUser";
            this.tsslUser.Size = new System.Drawing.Size(65, 17);
            this.tsslUser.Text = "Usuario: —";
            // 
            // tsslRole
            // 
            this.tsslRole.Name = "tsslRole";
            this.tsslRole.Size = new System.Drawing.Size(42, 17);
            this.tsslRole.Text = "Rol: —";
            // 
            // tsslPlant
            // 
            this.tsslPlant.Name = "tsslPlant";
            this.tsslPlant.Size = new System.Drawing.Size(58, 17);
            this.tsslPlant.Text = "Planta: —";
            // 
            // tsslSpring
            // 
            this.tsslSpring.Name = "tsslSpring";
            this.tsslSpring.Size = new System.Drawing.Size(796, 17);
            this.tsslSpring.Spring = true;
            // 
            // tsslEstado
            // 
            this.tsslEstado.Name = "tsslEstado";
            this.tsslEstado.Size = new System.Drawing.Size(32, 17);
            this.tsslEstado.Text = "Listo";
            // 
            // root
            // 
            this.root.ColumnCount = 1;
            this.root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.root.Controls.Add(this.pnlTopBar, 0, 0);
            this.root.Controls.Add(this.splitMain, 0, 1);
            this.root.Controls.Add(this.statusMain, 0, 2);
            this.root.Dock = System.Windows.Forms.DockStyle.Fill;
            this.root.Location = new System.Drawing.Point(0, 0);
            this.root.Name = "root";
            this.root.RowCount = 3;
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.root.Size = new System.Drawing.Size(1008, 601);
            this.root.TabIndex = 1;
            // 
            // pnlTopBar
            // 
            this.pnlTopBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pnlTopBar.Controls.Add(this.btnCerrarSesion);
            this.pnlTopBar.Controls.Add(this.lblTitulo);
            this.pnlTopBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTopBar.Location = new System.Drawing.Point(0, 0);
            this.pnlTopBar.Margin = new System.Windows.Forms.Padding(0);
            this.pnlTopBar.Name = "pnlTopBar";
            this.pnlTopBar.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.pnlTopBar.Size = new System.Drawing.Size(1008, 50);
            this.pnlTopBar.TabIndex = 0;
            // 
            // btnCerrarSesion
            // 
            this.btnCerrarSesion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrarSesion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnCerrarSesion.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.btnCerrarSesion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrarSesion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCerrarSesion.Location = new System.Drawing.Point(878, 10);
            this.btnCerrarSesion.Name = "btnCerrarSesion";
            this.btnCerrarSesion.Size = new System.Drawing.Size(120, 30);
            this.btnCerrarSesion.TabIndex = 1;
            this.btnCerrarSesion.Text = "Cerrar Sesión";
            this.btnCerrarSesion.UseVisualStyleBackColor = false;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.lblTitulo.Location = new System.Drawing.Point(10, 13);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(148, 25);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "GPI – Modulos.";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 601);
            this.Controls.Add(this.root);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1024, 640);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GPI – Lanzador de Módulos";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeft.PerformLayout();
            this.tabMain.ResumeLayout(false);
            this.tabDashboard.ResumeLayout(false);
            this.tlpDash.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLaunchLog)).EndInit();
            this.tabModulos.ResumeLayout(false);
            this.pnlModulosHeader.ResumeLayout(false);
            this.pnlModulosHeader.PerformLayout();
            this.tabModulosTest.ResumeLayout(false);
            this.pnlModulosTestHeader.ResumeLayout(false);
            this.pnlModulosTestHeader.PerformLayout();
            this.tabAdmin.ResumeLayout(false);
            this.pnlAdminButtons.ResumeLayout(false);
            this.splitAdmin.Panel1.ResumeLayout(false);
            this.splitAdmin.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitAdmin)).EndInit();
            this.splitAdmin.ResumeLayout(false);
            this.leftAdmin.ResumeLayout(false);
            this.grpRoles.ResumeLayout(false);
            this.barFiltroPlanta.ResumeLayout(false);
            this.barFiltroPlanta.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).EndInit();
            this.rightAdmin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvModulos)).EndInit();
            this.tabConfig.ResumeLayout(false);
            this.grpGeneral.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvModulesConfig)).EndInit();
            this.grpAcerca.ResumeLayout(false);
            this.cmuModulo.ResumeLayout(false);
            this.statusMain.ResumeLayout(false);
            this.statusMain.PerformLayout();
            this.root.ResumeLayout(false);
            this.root.PerformLayout();
            this.pnlTopBar.ResumeLayout(false);
            this.pnlTopBar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        // ✅ REMOVIDO: menuMain y toolMain

        private SplitContainer splitMain;
        private Panel pnlLeft;
        private Label lblFavoritos;
        private TextBox txtFiltroCategoria;
        private TreeView treeFavoritos;

        private TabControl tabMain;
        private TabPage tabDashboard;
        private TabPage tabModulos;  // ✅ NUEVO: Tab unificado
        private TabPage tabAdmin;
        private TabPage tabConfig;

        private TableLayoutPanel tlpDash;
        private FlowLayoutPanel flpKPIs;
        private DataGridView dgvLaunchLog;

        // ✅ NUEVO: Controles para tab unificado de módulos
        private Panel pnlModulosHeader;
        private Label lblModulosTitulo;
        private TextBox txtModulosSearch;
        private Button btnModulosRefrescar;
        private FlowLayoutPanel flpModulos;

        private SplitContainer splitAdmin;
        private GroupBox grpRoles;
        private DataGridView dgvUsuarios;
        private DataGridView dgvModulos;

        private GroupBox grpGeneral;
        private GroupBox grpAcerca;
        private Label lblVersion;

        private ContextMenuStrip cmuModulo;
        private ToolStripMenuItem cmuAbrir;
        private ToolStripMenuItem cmuAbrirAdmin;
        private ToolStripMenuItem cmuCopiarRuta;
        private ToolStripMenuItem cmuVerProp;

        private StatusStrip statusMain;
        private ToolStripStatusLabel tsslUser;
        private ToolStripStatusLabel tsslRole;
        private ToolStripStatusLabel tsslPlant;
        private ToolStripStatusLabel tsslSpring;
        private ToolStripStatusLabel tsslEstado;
        private Panel leftAdmin;
        private Panel rightAdmin;
        private TableLayoutPanel root;
        private System.Windows.Forms.Panel pnlAdminButtons;
        private System.Windows.Forms.Button btnAdminGuardar;
        private System.Windows.Forms.Button btnAdminRefrescar;
        private Button btnDeleteModule;
        private Button btnSaveModule;
        private Button btnNewModule;
        private DataGridView dgvModulesConfig;
        private FlowLayoutPanel barFiltroPlanta;
        private CheckBox chkPlantFilter;
        private ComboBox cboPlantFilter;

        // ✅ NUEVO: Barra superior con botón de cerrar sesión
        private Panel pnlTopBar;
        private Label lblTitulo;
        private Button btnCerrarSesion;

        private TabPage tabModulosTest;
        private Panel pnlModulosTestHeader;
        private Label lblModulosTestTitulo;
        private TextBox txtModulosTestSearch;
        private Button btnModulosTestRefrescar;
        private FlowLayoutPanel flpModulosTest;


    }
}
