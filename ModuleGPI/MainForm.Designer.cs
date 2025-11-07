using System.Drawing;
using System.Windows.Forms;

namespace GPI.Launcher
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuMain = new System.Windows.Forms.MenuStrip();
            this.mnuArchivo = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuArchivo_Salir = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVer = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVer_Refrescar = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHerramientas = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHerramientas_Config = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAyuda = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAyuda_Acerca = new System.Windows.Forms.ToolStripMenuItem();
            this.toolMain = new System.Windows.Forms.ToolStrip();
            this.tsbRefrescar = new System.Windows.Forms.ToolStripButton();
            this.tss1 = new System.Windows.Forms.ToolStripSeparator();
            this.tstBuscar = new System.Windows.Forms.ToolStripTextBox();
            this.tsbBuscar = new System.Windows.Forms.ToolStripButton();
            this.tss2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbConfig = new System.Windows.Forms.ToolStripButton();
            this.tsbCerrarSesion = new System.Windows.Forms.ToolStripButton();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.tvCategorias = new System.Windows.Forms.TreeView();
            this.txtFiltroCategoria = new System.Windows.Forms.TextBox();
            this.lblCategorias = new System.Windows.Forms.Label();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabDashboard = new System.Windows.Forms.TabPage();
            this.tlpDash = new System.Windows.Forms.TableLayoutPanel();
            this.flpKPIs = new System.Windows.Forms.FlowLayoutPanel();
            this.dgvLaunchLog = new System.Windows.Forms.DataGridView();
            this.tabOperacion = new System.Windows.Forms.TabPage();
            this.flpOperacion = new System.Windows.Forms.FlowLayoutPanel();
            this.btnMod_Op_Inventario = new System.Windows.Forms.Button();
            this.btnMod_Op_Logistica = new System.Windows.Forms.Button();
            this.btnMod_Op_Produccion = new System.Windows.Forms.Button();
            this.btnMod_Op_MP = new System.Windows.Forms.Button();
            this.btnMod_Op_corrugado = new System.Windows.Forms.Button();
            this.btnMod_Op_CM = new System.Windows.Forms.Button();
            this.btnMod_Op_ConversorUM = new System.Windows.Forms.Button();
            this.btnMod_Op_CalculadoraTarimas = new System.Windows.Forms.Button();
            this.pnlOpHeader = new System.Windows.Forms.Panel();
            this.lblOpTitulo = new System.Windows.Forms.Label();
            this.txtOpSearch = new System.Windows.Forms.TextBox();
            this.btnOpRefrescar = new System.Windows.Forms.Button();
            this.tabConsultas = new System.Windows.Forms.TabPage();
            this.flpConsultas = new System.Windows.Forms.FlowLayoutPanel();
            this.btnMod_Cons_Reportes = new System.Windows.Forms.Button();
            this.btnMod_Cons_KPIs = new System.Windows.Forms.Button();
            this.pnlConsHeader = new System.Windows.Forms.Panel();
            this.lblConsTitulo = new System.Windows.Forms.Label();
            this.txtConsSearch = new System.Windows.Forms.TextBox();
            this.tabAdmin = new System.Windows.Forms.TabPage();
            this.pnlAdminButtons = new System.Windows.Forms.Panel();
            this.btnAdminGuardar = new System.Windows.Forms.Button();
            this.btnAdminRefrescar = new System.Windows.Forms.Button();
            this.splitAdmin = new System.Windows.Forms.SplitContainer();
            this.leftAdmin = new System.Windows.Forms.Panel();
            this.dgvUsuarios = new System.Windows.Forms.DataGridView();
            this.grpRoles = new System.Windows.Forms.GroupBox();
            this.rightAdmin = new System.Windows.Forms.Panel();
            this.dgvModulos = new System.Windows.Forms.DataGridView();
            this.tabConfig = new System.Windows.Forms.TabPage();
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.chkRutasPermitidas = new System.Windows.Forms.CheckedListBox();
            this.grpAcerca = new System.Windows.Forms.GroupBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnGuardarConfig = new System.Windows.Forms.Button();
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
            this.menuMain.SuspendLayout();
            this.toolMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabDashboard.SuspendLayout();
            this.tlpDash.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLaunchLog)).BeginInit();
            this.tabOperacion.SuspendLayout();
            this.flpOperacion.SuspendLayout();
            this.pnlOpHeader.SuspendLayout();
            this.tabConsultas.SuspendLayout();
            this.flpConsultas.SuspendLayout();
            this.pnlConsHeader.SuspendLayout();
            this.tabAdmin.SuspendLayout();
            this.pnlAdminButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitAdmin)).BeginInit();
            this.splitAdmin.Panel1.SuspendLayout();
            this.splitAdmin.Panel2.SuspendLayout();
            this.splitAdmin.SuspendLayout();
            this.leftAdmin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).BeginInit();
            this.rightAdmin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvModulos)).BeginInit();
            this.tabConfig.SuspendLayout();
            this.grpGeneral.SuspendLayout();
            this.grpAcerca.SuspendLayout();
            this.cmuModulo.SuspendLayout();
            this.statusMain.SuspendLayout();
            this.root.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuMain
            // 
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuArchivo,
            this.mnuVer,
            this.mnuHerramientas,
            this.mnuAyuda});
            this.menuMain.Location = new System.Drawing.Point(0, 0);
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(1008, 24);
            this.menuMain.TabIndex = 0;
            // 
            // mnuArchivo
            // 
            this.mnuArchivo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuArchivo_Salir});
            this.mnuArchivo.Name = "mnuArchivo";
            this.mnuArchivo.Size = new System.Drawing.Size(60, 20);
            this.mnuArchivo.Text = "Archivo";
            // 
            // mnuArchivo_Salir
            // 
            this.mnuArchivo_Salir.Name = "mnuArchivo_Salir";
            this.mnuArchivo_Salir.Size = new System.Drawing.Size(96, 22);
            this.mnuArchivo_Salir.Text = "Salir";
            // 
            // mnuVer
            // 
            this.mnuVer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuVer_Refrescar});
            this.mnuVer.Name = "mnuVer";
            this.mnuVer.Size = new System.Drawing.Size(35, 20);
            this.mnuVer.Text = "Ver";
            // 
            // mnuVer_Refrescar
            // 
            this.mnuVer_Refrescar.Name = "mnuVer_Refrescar";
            this.mnuVer_Refrescar.Size = new System.Drawing.Size(122, 22);
            this.mnuVer_Refrescar.Text = "Refrescar";
            // 
            // mnuHerramientas
            // 
            this.mnuHerramientas.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHerramientas_Config});
            this.mnuHerramientas.Name = "mnuHerramientas";
            this.mnuHerramientas.Size = new System.Drawing.Size(90, 20);
            this.mnuHerramientas.Text = "Herramientas";
            // 
            // mnuHerramientas_Config
            // 
            this.mnuHerramientas_Config.Name = "mnuHerramientas_Config";
            this.mnuHerramientas_Config.Size = new System.Drawing.Size(150, 22);
            this.mnuHerramientas_Config.Text = "Configuración";
            // 
            // mnuAyuda
            // 
            this.mnuAyuda.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAyuda_Acerca});
            this.mnuAyuda.Name = "mnuAyuda";
            this.mnuAyuda.Size = new System.Drawing.Size(53, 20);
            this.mnuAyuda.Text = "Ayuda";
            // 
            // mnuAyuda_Acerca
            // 
            this.mnuAyuda_Acerca.Name = "mnuAyuda_Acerca";
            this.mnuAyuda_Acerca.Size = new System.Drawing.Size(135, 22);
            this.mnuAyuda_Acerca.Text = "Acerca de…";
            // 
            // toolMain
            // 
            this.toolMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbRefrescar,
            this.tss1,
            this.tstBuscar,
            this.tsbBuscar,
            this.tss2,
            this.tsbConfig,
            this.tsbCerrarSesion});
            this.toolMain.Location = new System.Drawing.Point(0, 24);
            this.toolMain.Name = "toolMain";
            this.toolMain.Size = new System.Drawing.Size(1008, 25);
            this.toolMain.TabIndex = 1;
            // 
            // tsbRefrescar
            // 
            this.tsbRefrescar.Name = "tsbRefrescar";
            this.tsbRefrescar.Size = new System.Drawing.Size(59, 22);
            this.tsbRefrescar.Text = "Refrescar";
            // 
            // tss1
            // 
            this.tss1.Name = "tss1";
            this.tss1.Size = new System.Drawing.Size(6, 25);
            // 
            // tstBuscar
            // 
            this.tstBuscar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tstBuscar.Name = "tstBuscar";
            this.tstBuscar.Size = new System.Drawing.Size(220, 25);
            this.tstBuscar.ToolTipText = "Buscar módulo…";
            // 
            // tsbBuscar
            // 
            this.tsbBuscar.Name = "tsbBuscar";
            this.tsbBuscar.Size = new System.Drawing.Size(46, 22);
            this.tsbBuscar.Text = "Buscar";
            // 
            // tss2
            // 
            this.tss2.Name = "tss2";
            this.tss2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbConfig
            // 
            this.tsbConfig.Name = "tsbConfig";
            this.tsbConfig.Size = new System.Drawing.Size(47, 22);
            this.tsbConfig.Text = "Config";
            // 
            // tsbCerrarSesion
            // 
            this.tsbCerrarSesion.Name = "tsbCerrarSesion";
            this.tsbCerrarSesion.Size = new System.Drawing.Size(79, 22);
            this.tsbCerrarSesion.Text = "Cerrar sesión";
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.Location = new System.Drawing.Point(3, 52);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.pnlLeft);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.tabMain);
            this.splitMain.Size = new System.Drawing.Size(1002, 524);
            this.splitMain.SplitterDistance = 121;
            this.splitMain.TabIndex = 2;
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.tvCategorias);
            this.pnlLeft.Controls.Add(this.txtFiltroCategoria);
            this.pnlLeft.Controls.Add(this.lblCategorias);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(121, 524);
            this.pnlLeft.TabIndex = 0;
            // 
            // tvCategorias
            // 
            this.tvCategorias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvCategorias.Location = new System.Drawing.Point(0, 44);
            this.tvCategorias.Name = "tvCategorias";
            this.tvCategorias.Size = new System.Drawing.Size(121, 480);
            this.tvCategorias.TabIndex = 0;
            // 
            // txtFiltroCategoria
            // 
            this.txtFiltroCategoria.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtFiltroCategoria.Location = new System.Drawing.Point(0, 24);
            this.txtFiltroCategoria.Name = "txtFiltroCategoria";
            this.txtFiltroCategoria.Size = new System.Drawing.Size(121, 20);
            this.txtFiltroCategoria.TabIndex = 1;
            this.txtFiltroCategoria.Tag = "Filtrar categorías…";
            // 
            // lblCategorias
            // 
            this.lblCategorias.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCategorias.Location = new System.Drawing.Point(0, 0);
            this.lblCategorias.Name = "lblCategorias";
            this.lblCategorias.Padding = new System.Windows.Forms.Padding(6, 6, 0, 0);
            this.lblCategorias.Size = new System.Drawing.Size(121, 24);
            this.lblCategorias.TabIndex = 2;
            this.lblCategorias.Text = "Categorías";
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabDashboard);
            this.tabMain.Controls.Add(this.tabOperacion);
            this.tabMain.Controls.Add(this.tabConsultas);
            this.tabMain.Controls.Add(this.tabAdmin);
            this.tabMain.Controls.Add(this.tabConfig);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(877, 524);
            this.tabMain.TabIndex = 0;
            // 
            // tabDashboard
            // 
            this.tabDashboard.Controls.Add(this.tlpDash);
            this.tabDashboard.Location = new System.Drawing.Point(4, 22);
            this.tabDashboard.Name = "tabDashboard";
            this.tabDashboard.Size = new System.Drawing.Size(869, 498);
            this.tabDashboard.TabIndex = 0;
            this.tabDashboard.Text = "Dashboard";
            // 
            // tlpDash
            // 
            this.tlpDash.ColumnCount = 1;
            this.tlpDash.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDash.Controls.Add(this.flpKPIs, 0, 0);
            this.tlpDash.Controls.Add(this.dgvLaunchLog, 0, 1);
            this.tlpDash.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDash.Location = new System.Drawing.Point(0, 0);
            this.tlpDash.Name = "tlpDash";
            this.tlpDash.RowCount = 2;
            this.tlpDash.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpDash.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDash.Size = new System.Drawing.Size(869, 498);
            this.tlpDash.TabIndex = 0;
            // 
            // flpKPIs
            // 
            this.flpKPIs.AutoScroll = true;
            this.flpKPIs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpKPIs.Location = new System.Drawing.Point(3, 3);
            this.flpKPIs.Name = "flpKPIs";
            this.flpKPIs.Padding = new System.Windows.Forms.Padding(8);
            this.flpKPIs.Size = new System.Drawing.Size(863, 114);
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
            this.dgvLaunchLog.Size = new System.Drawing.Size(863, 372);
            this.dgvLaunchLog.TabIndex = 1;
            // 
            // tabOperacion
            // 
            this.tabOperacion.Controls.Add(this.flpOperacion);
            this.tabOperacion.Controls.Add(this.pnlOpHeader);
            this.tabOperacion.Location = new System.Drawing.Point(4, 22);
            this.tabOperacion.Name = "tabOperacion";
            this.tabOperacion.Size = new System.Drawing.Size(869, 498);
            this.tabOperacion.TabIndex = 1;
            this.tabOperacion.Text = "Operación";
            // 
            // flpOperacion
            // 
            this.flpOperacion.AutoScroll = true;
            this.flpOperacion.Controls.Add(this.btnMod_Op_Inventario);
            this.flpOperacion.Controls.Add(this.btnMod_Op_Logistica);
            this.flpOperacion.Controls.Add(this.btnMod_Op_Produccion);
            this.flpOperacion.Controls.Add(this.btnMod_Op_MP);
            this.flpOperacion.Controls.Add(this.btnMod_Op_corrugado);
            this.flpOperacion.Controls.Add(this.btnMod_Op_CM);
            this.flpOperacion.Controls.Add(this.btnMod_Op_ConversorUM);
            this.flpOperacion.Controls.Add(this.btnMod_Op_CalculadoraTarimas);
            this.flpOperacion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpOperacion.Location = new System.Drawing.Point(0, 48);
            this.flpOperacion.Name = "flpOperacion";
            this.flpOperacion.Padding = new System.Windows.Forms.Padding(8);
            this.flpOperacion.Size = new System.Drawing.Size(869, 450);
            this.flpOperacion.TabIndex = 0;
            // 
            // btnMod_Op_Inventario
            // 
            this.btnMod_Op_Inventario.Location = new System.Drawing.Point(16, 16);
            this.btnMod_Op_Inventario.Margin = new System.Windows.Forms.Padding(8);
            this.btnMod_Op_Inventario.Name = "btnMod_Op_Inventario";
            this.btnMod_Op_Inventario.Size = new System.Drawing.Size(200, 48);
            this.btnMod_Op_Inventario.TabIndex = 0;
            this.btnMod_Op_Inventario.Text = "Conteo Physic";
            // 
            // btnMod_Op_Logistica
            // 
            this.btnMod_Op_Logistica.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnMod_Op_Logistica.Location = new System.Drawing.Point(232, 16);
            this.btnMod_Op_Logistica.Margin = new System.Windows.Forms.Padding(8);
            this.btnMod_Op_Logistica.Name = "btnMod_Op_Logistica";
            this.btnMod_Op_Logistica.Size = new System.Drawing.Size(200, 48);
            this.btnMod_Op_Logistica.TabIndex = 1;
            this.btnMod_Op_Logistica.Text = "SAP crd";
            this.btnMod_Op_Logistica.UseVisualStyleBackColor = false;
            // 
            // btnMod_Op_Produccion
            // 
            this.btnMod_Op_Produccion.Location = new System.Drawing.Point(448, 16);
            this.btnMod_Op_Produccion.Margin = new System.Windows.Forms.Padding(8);
            this.btnMod_Op_Produccion.Name = "btnMod_Op_Produccion";
            this.btnMod_Op_Produccion.Size = new System.Drawing.Size(200, 48);
            this.btnMod_Op_Produccion.TabIndex = 2;
            this.btnMod_Op_Produccion.Text = "Carta Porte";
            // 
            // btnMod_Op_MP
            // 
            this.btnMod_Op_MP.Location = new System.Drawing.Point(16, 80);
            this.btnMod_Op_MP.Margin = new System.Windows.Forms.Padding(8);
            this.btnMod_Op_MP.Name = "btnMod_Op_MP";
            this.btnMod_Op_MP.Size = new System.Drawing.Size(200, 48);
            this.btnMod_Op_MP.TabIndex = 3;
            this.btnMod_Op_MP.Text = "Etiquetas MP";
            // 
            // btnMod_Op_corrugado
            // 
            this.btnMod_Op_corrugado.Location = new System.Drawing.Point(232, 80);
            this.btnMod_Op_corrugado.Margin = new System.Windows.Forms.Padding(8);
            this.btnMod_Op_corrugado.Name = "btnMod_Op_corrugado";
            this.btnMod_Op_corrugado.Size = new System.Drawing.Size(200, 48);
            this.btnMod_Op_corrugado.TabIndex = 4;
            this.btnMod_Op_corrugado.Text = "Etiquetas Corrugado";
            // 
            // btnMod_Op_CM
            // 
            this.btnMod_Op_CM.Location = new System.Drawing.Point(448, 80);
            this.btnMod_Op_CM.Margin = new System.Windows.Forms.Padding(8);
            this.btnMod_Op_CM.Name = "btnMod_Op_CM";
            this.btnMod_Op_CM.Size = new System.Drawing.Size(200, 48);
            this.btnMod_Op_CM.TabIndex = 5;
            this.btnMod_Op_CM.Text = "Etiquetas CM";
            // 
            // btnMod_Op_ConversorUM
            // 
            this.btnMod_Op_ConversorUM.Location = new System.Drawing.Point(16, 144);
            this.btnMod_Op_ConversorUM.Margin = new System.Windows.Forms.Padding(8);
            this.btnMod_Op_ConversorUM.Name = "btnMod_Op_ConversorUM";
            this.btnMod_Op_ConversorUM.Size = new System.Drawing.Size(200, 48);
            this.btnMod_Op_ConversorUM.TabIndex = 6;
            this.btnMod_Op_ConversorUM.Text = "Conversor UM";
            // 
            // btnMod_Op_CalculadoraTarimas
            // 
            this.btnMod_Op_CalculadoraTarimas.Location = new System.Drawing.Point(232, 144);
            this.btnMod_Op_CalculadoraTarimas.Margin = new System.Windows.Forms.Padding(8);
            this.btnMod_Op_CalculadoraTarimas.Name = "btnMod_Op_CalculadoraTarimas";
            this.btnMod_Op_CalculadoraTarimas.Size = new System.Drawing.Size(200, 48);
            this.btnMod_Op_CalculadoraTarimas.TabIndex = 7;
            this.btnMod_Op_CalculadoraTarimas.Text = "Calculadora Tarimas";
            // 
            // pnlOpHeader
            // 
            this.pnlOpHeader.Controls.Add(this.lblOpTitulo);
            this.pnlOpHeader.Controls.Add(this.txtOpSearch);
            this.pnlOpHeader.Controls.Add(this.btnOpRefrescar);
            this.pnlOpHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOpHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlOpHeader.Name = "pnlOpHeader";
            this.pnlOpHeader.Padding = new System.Windows.Forms.Padding(8);
            this.pnlOpHeader.Size = new System.Drawing.Size(869, 48);
            this.pnlOpHeader.TabIndex = 1;
            // 
            // lblOpTitulo
            // 
            this.lblOpTitulo.AutoSize = true;
            this.lblOpTitulo.Location = new System.Drawing.Point(8, 14);
            this.lblOpTitulo.Name = "lblOpTitulo";
            this.lblOpTitulo.Size = new System.Drawing.Size(114, 13);
            this.lblOpTitulo.TabIndex = 0;
            this.lblOpTitulo.Text = "Módulos de Operación";
            // 
            // txtOpSearch
            // 
            this.txtOpSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOpSearch.Location = new System.Drawing.Point(527, 12);
            this.txtOpSearch.Name = "txtOpSearch";
            this.txtOpSearch.Size = new System.Drawing.Size(220, 20);
            this.txtOpSearch.TabIndex = 1;
            // 
            // btnOpRefrescar
            // 
            this.btnOpRefrescar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpRefrescar.Location = new System.Drawing.Point(755, 11);
            this.btnOpRefrescar.Name = "btnOpRefrescar";
            this.btnOpRefrescar.Size = new System.Drawing.Size(90, 23);
            this.btnOpRefrescar.TabIndex = 2;
            this.btnOpRefrescar.Text = "Refrescar";
            // 
            // tabConsultas
            // 
            this.tabConsultas.Controls.Add(this.flpConsultas);
            this.tabConsultas.Controls.Add(this.pnlConsHeader);
            this.tabConsultas.Location = new System.Drawing.Point(4, 22);
            this.tabConsultas.Name = "tabConsultas";
            this.tabConsultas.Size = new System.Drawing.Size(869, 498);
            this.tabConsultas.TabIndex = 2;
            this.tabConsultas.Text = "Consultas";
            // 
            // flpConsultas
            // 
            this.flpConsultas.AutoScroll = true;
            this.flpConsultas.Controls.Add(this.btnMod_Cons_Reportes);
            this.flpConsultas.Controls.Add(this.btnMod_Cons_KPIs);
            this.flpConsultas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpConsultas.Location = new System.Drawing.Point(0, 48);
            this.flpConsultas.Name = "flpConsultas";
            this.flpConsultas.Padding = new System.Windows.Forms.Padding(8);
            this.flpConsultas.Size = new System.Drawing.Size(869, 450);
            this.flpConsultas.TabIndex = 0;
            // 
            // btnMod_Cons_Reportes
            // 
            this.btnMod_Cons_Reportes.Location = new System.Drawing.Point(16, 16);
            this.btnMod_Cons_Reportes.Margin = new System.Windows.Forms.Padding(8);
            this.btnMod_Cons_Reportes.Name = "btnMod_Cons_Reportes";
            this.btnMod_Cons_Reportes.Size = new System.Drawing.Size(200, 48);
            this.btnMod_Cons_Reportes.TabIndex = 0;
            this.btnMod_Cons_Reportes.Text = "Reportes";
            // 
            // btnMod_Cons_KPIs
            // 
            this.btnMod_Cons_KPIs.Location = new System.Drawing.Point(232, 16);
            this.btnMod_Cons_KPIs.Margin = new System.Windows.Forms.Padding(8);
            this.btnMod_Cons_KPIs.Name = "btnMod_Cons_KPIs";
            this.btnMod_Cons_KPIs.Size = new System.Drawing.Size(200, 48);
            this.btnMod_Cons_KPIs.TabIndex = 1;
            this.btnMod_Cons_KPIs.Text = "KPIs";
            // 
            // pnlConsHeader
            // 
            this.pnlConsHeader.Controls.Add(this.lblConsTitulo);
            this.pnlConsHeader.Controls.Add(this.txtConsSearch);
            this.pnlConsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlConsHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlConsHeader.Name = "pnlConsHeader";
            this.pnlConsHeader.Padding = new System.Windows.Forms.Padding(8);
            this.pnlConsHeader.Size = new System.Drawing.Size(869, 48);
            this.pnlConsHeader.TabIndex = 1;
            // 
            // lblConsTitulo
            // 
            this.lblConsTitulo.AutoSize = true;
            this.lblConsTitulo.Location = new System.Drawing.Point(8, 14);
            this.lblConsTitulo.Name = "lblConsTitulo";
            this.lblConsTitulo.Size = new System.Drawing.Size(106, 13);
            this.lblConsTitulo.TabIndex = 0;
            this.lblConsTitulo.Text = "Módulos de Consulta";
            // 
            // txtConsSearch
            // 
            this.txtConsSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConsSearch.Location = new System.Drawing.Point(641, 12);
            this.txtConsSearch.Name = "txtConsSearch";
            this.txtConsSearch.Size = new System.Drawing.Size(220, 20);
            this.txtConsSearch.TabIndex = 1;
            // 
            // tabAdmin
            // 
            this.tabAdmin.Controls.Add(this.pnlAdminButtons);
            this.tabAdmin.Controls.Add(this.splitAdmin);
            this.tabAdmin.Location = new System.Drawing.Point(4, 22);
            this.tabAdmin.Name = "tabAdmin";
            this.tabAdmin.Size = new System.Drawing.Size(869, 498);
            this.tabAdmin.TabIndex = 3;
            this.tabAdmin.Text = "Administración";
            // 
            // pnlAdminButtons
            // 
            this.pnlAdminButtons.Controls.Add(this.btnAdminGuardar);
            this.pnlAdminButtons.Controls.Add(this.btnAdminRefrescar);
            this.pnlAdminButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlAdminButtons.Location = new System.Drawing.Point(0, 454);
            this.pnlAdminButtons.Name = "pnlAdminButtons";
            this.pnlAdminButtons.Padding = new System.Windows.Forms.Padding(8);
            this.pnlAdminButtons.Size = new System.Drawing.Size(869, 44);
            this.pnlAdminButtons.TabIndex = 1;
            // 
            // btnAdminGuardar
            // 
            this.btnAdminGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdminGuardar.Location = new System.Drawing.Point(761, 8);
            this.btnAdminGuardar.Name = "btnAdminGuardar";
            this.btnAdminGuardar.Size = new System.Drawing.Size(100, 28);
            this.btnAdminGuardar.TabIndex = 0;
            this.btnAdminGuardar.Text = "Guardar";
            // 
            // btnAdminRefrescar
            // 
            this.btnAdminRefrescar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdminRefrescar.Location = new System.Drawing.Point(653, 8);
            this.btnAdminRefrescar.Name = "btnAdminRefrescar";
            this.btnAdminRefrescar.Size = new System.Drawing.Size(100, 28);
            this.btnAdminRefrescar.TabIndex = 1;
            this.btnAdminRefrescar.Text = "Refrescar";
            // 
            // splitAdmin
            // 
            this.splitAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.splitAdmin.Size = new System.Drawing.Size(869, 498);
            this.splitAdmin.SplitterDistance = 695;
            this.splitAdmin.TabIndex = 0;
            // 
            // leftAdmin
            // 
            this.leftAdmin.Controls.Add(this.dgvUsuarios);
            this.leftAdmin.Controls.Add(this.grpRoles);
            this.leftAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftAdmin.Location = new System.Drawing.Point(0, 0);
            this.leftAdmin.Name = "leftAdmin";
            this.leftAdmin.Size = new System.Drawing.Size(695, 498);
            this.leftAdmin.TabIndex = 0;
            // 
            // dgvUsuarios
            // 
            this.dgvUsuarios.AllowUserToAddRows = false;
            this.dgvUsuarios.AllowUserToDeleteRows = false;
            this.dgvUsuarios.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUsuarios.Location = new System.Drawing.Point(0, 56);
            this.dgvUsuarios.Name = "dgvUsuarios";
            this.dgvUsuarios.ReadOnly = true;
            this.dgvUsuarios.Size = new System.Drawing.Size(695, 442);
            this.dgvUsuarios.TabIndex = 0;
            // 
            // grpRoles
            // 
            this.grpRoles.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpRoles.Location = new System.Drawing.Point(0, 0);
            this.grpRoles.Name = "grpRoles";
            this.grpRoles.Padding = new System.Windows.Forms.Padding(8);
            this.grpRoles.Size = new System.Drawing.Size(695, 56);
            this.grpRoles.TabIndex = 1;
            this.grpRoles.TabStop = false;
            this.grpRoles.Text = "Roles y Usuarios";
            // 
            // rightAdmin
            // 
            this.rightAdmin.Controls.Add(this.dgvModulos);
            this.rightAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightAdmin.Location = new System.Drawing.Point(0, 0);
            this.rightAdmin.Name = "rightAdmin";
            this.rightAdmin.Size = new System.Drawing.Size(170, 498);
            this.rightAdmin.TabIndex = 0;
            // 
            // dgvModulos
            // 
            this.dgvModulos.AllowUserToAddRows = false;
            this.dgvModulos.AllowUserToDeleteRows = false;
            this.dgvModulos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvModulos.Location = new System.Drawing.Point(0, 0);
            this.dgvModulos.Name = "dgvModulos";
            this.dgvModulos.ReadOnly = true;
            this.dgvModulos.Size = new System.Drawing.Size(170, 498);
            this.dgvModulos.TabIndex = 0;
            // 
            // tabConfig
            // 
            this.tabConfig.Controls.Add(this.grpGeneral);
            this.tabConfig.Controls.Add(this.grpAcerca);
            this.tabConfig.Location = new System.Drawing.Point(4, 22);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.Size = new System.Drawing.Size(869, 498);
            this.tabConfig.TabIndex = 4;
            this.tabConfig.Text = "Configuración";
            // 
            // grpGeneral
            // 
            this.grpGeneral.Controls.Add(this.chkRutasPermitidas);
            this.grpGeneral.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpGeneral.Location = new System.Drawing.Point(0, 0);
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Padding = new System.Windows.Forms.Padding(8);
            this.grpGeneral.Size = new System.Drawing.Size(869, 160);
            this.grpGeneral.TabIndex = 0;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "General";
            // 
            // chkRutasPermitidas
            // 
            this.chkRutasPermitidas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRutasPermitidas.Location = new System.Drawing.Point(8, 21);
            this.chkRutasPermitidas.Name = "chkRutasPermitidas";
            this.chkRutasPermitidas.Size = new System.Drawing.Size(853, 131);
            this.chkRutasPermitidas.TabIndex = 0;
            // 
            // grpAcerca
            // 
            this.grpAcerca.Controls.Add(this.lblVersion);
            this.grpAcerca.Controls.Add(this.btnGuardarConfig);
            this.grpAcerca.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grpAcerca.Location = new System.Drawing.Point(0, 378);
            this.grpAcerca.Name = "grpAcerca";
            this.grpAcerca.Padding = new System.Windows.Forms.Padding(8);
            this.grpAcerca.Size = new System.Drawing.Size(869, 120);
            this.grpAcerca.TabIndex = 1;
            this.grpAcerca.TabStop = false;
            this.grpAcerca.Text = "Acerca de";
            // 
            // lblVersion
            // 
            this.lblVersion.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblVersion.Location = new System.Drawing.Point(8, 21);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(853, 24);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "Versión: 1.0.0";
            // 
            // btnGuardarConfig
            // 
            this.btnGuardarConfig.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnGuardarConfig.Enabled = false;
            this.btnGuardarConfig.Location = new System.Drawing.Point(8, 76);
            this.btnGuardarConfig.Name = "btnGuardarConfig";
            this.btnGuardarConfig.Size = new System.Drawing.Size(853, 36);
            this.btnGuardarConfig.TabIndex = 1;
            this.btnGuardarConfig.Text = "Guardar";
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
            this.root.Controls.Add(this.menuMain, 0, 0);
            this.root.Controls.Add(this.toolMain, 0, 1);
            this.root.Controls.Add(this.splitMain, 0, 2);
            this.root.Controls.Add(this.statusMain, 0, 3);
            this.root.Dock = System.Windows.Forms.DockStyle.Fill;
            this.root.Location = new System.Drawing.Point(0, 0);
            this.root.Name = "root";
            this.root.RowCount = 4;
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.root.Size = new System.Drawing.Size(1008, 601);
            this.root.TabIndex = 1;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1008, 601);
            this.Controls.Add(this.root);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1024, 640);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GPI – Lanzador de Módulos";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.menuMain.ResumeLayout(false);
            this.menuMain.PerformLayout();
            this.toolMain.ResumeLayout(false);
            this.toolMain.PerformLayout();
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
            this.tabOperacion.ResumeLayout(false);
            this.flpOperacion.ResumeLayout(false);
            this.pnlOpHeader.ResumeLayout(false);
            this.pnlOpHeader.PerformLayout();
            this.tabConsultas.ResumeLayout(false);
            this.flpConsultas.ResumeLayout(false);
            this.pnlConsHeader.ResumeLayout(false);
            this.pnlConsHeader.PerformLayout();
            this.tabAdmin.ResumeLayout(false);
            this.pnlAdminButtons.ResumeLayout(false);
            this.splitAdmin.Panel1.ResumeLayout(false);
            this.splitAdmin.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitAdmin)).EndInit();
            this.splitAdmin.ResumeLayout(false);
            this.leftAdmin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).EndInit();
            this.rightAdmin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvModulos)).EndInit();
            this.tabConfig.ResumeLayout(false);
            this.grpGeneral.ResumeLayout(false);
            this.grpAcerca.ResumeLayout(false);
            this.cmuModulo.ResumeLayout(false);
            this.statusMain.ResumeLayout(false);
            this.statusMain.PerformLayout();
            this.root.ResumeLayout(false);
            this.root.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MenuStrip menuMain;
        private ToolStripMenuItem mnuArchivo;
        private ToolStripMenuItem mnuArchivo_Salir;
        private ToolStripMenuItem mnuVer;
        private ToolStripMenuItem mnuVer_Refrescar;
        private ToolStripMenuItem mnuHerramientas;
        private ToolStripMenuItem mnuHerramientas_Config;
        private ToolStripMenuItem mnuAyuda;
        private ToolStripMenuItem mnuAyuda_Acerca;

        private ToolStrip toolMain;
        private ToolStripButton tsbRefrescar;
        private ToolStripSeparator tss1;
        private ToolStripTextBox tstBuscar;
        private ToolStripButton tsbBuscar;
        private ToolStripSeparator tss2;
        private ToolStripButton tsbConfig;
        private ToolStripButton tsbCerrarSesion;

        private SplitContainer splitMain;
        private Panel pnlLeft;
        private Label lblCategorias;
        private TextBox txtFiltroCategoria;
        private TreeView tvCategorias;

        private TabControl tabMain;
        private TabPage tabDashboard;
        private TabPage tabOperacion;
        private TabPage tabConsultas;
        private TabPage tabAdmin;
        private TabPage tabConfig;

        private TableLayoutPanel tlpDash;
        private FlowLayoutPanel flpKPIs;
        private DataGridView dgvLaunchLog;

        private Panel pnlOpHeader;
        private Label lblOpTitulo;
        private TextBox txtOpSearch;
        private Button btnOpRefrescar;
        private FlowLayoutPanel flpOperacion;
        private Button btnMod_Op_Inventario;
        private Button btnMod_Op_Logistica;
        private Button btnMod_Op_Produccion;

        private Panel pnlConsHeader;
        private Label lblConsTitulo;
        private TextBox txtConsSearch;
        private FlowLayoutPanel flpConsultas;
        private Button btnMod_Cons_Reportes;
        private Button btnMod_Cons_KPIs;

        private SplitContainer splitAdmin;
        private GroupBox grpRoles;
        private DataGridView dgvUsuarios;
        //private GroupBox grpModulos;
        private DataGridView dgvModulos;

        private GroupBox grpGeneral;
        private CheckedListBox chkRutasPermitidas;
        private GroupBox grpAcerca;
        private Label lblVersion;
        private Button btnGuardarConfig;

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
        private Button btnMod_Op_MP;
        private Button btnMod_Op_corrugado;
        private Button btnMod_Op_CM;
        private Button btnMod_Op_ConversorUM;
        private Button btnMod_Op_CalculadoraTarimas;
    }
}