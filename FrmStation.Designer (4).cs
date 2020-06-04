namespace ExpPt1
{
    partial class FrmStation
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.txtBoxStationId = new System.Windows.Forms.TextBox();
            this.lblStationId = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblLine = new System.Windows.Forms.Label();
            this.lblStationName = new System.Windows.Forms.Label();
            this.txtBoxStationName = new System.Windows.Forms.TextBox();
            this.lblxlsDetLock = new System.Windows.Forms.Label();
            this.btnDetLock = new System.Windows.Forms.Button();
            this.lblxlsFP = new System.Windows.Forms.Label();
            this.btnFP = new System.Windows.Forms.Button();
            this.lblxlsEmSg = new System.Windows.Forms.Label();
            this.btnEmSg = new System.Windows.Forms.Button();
            this.lblxlsSpProf = new System.Windows.Forms.Label();
            this.btnSpProf = new System.Windows.Forms.Button();
            this.lblxlsRoutes = new System.Windows.Forms.Label();
            this.btnRoutes = new System.Windows.Forms.Button();
            this.lblxlsCmpRoutes = new System.Windows.Forms.Label();
            this.btnCmpRoutes = new System.Windows.Forms.Button();
            this.lblxlsLxs = new System.Windows.Forms.Label();
            this.btnLXs = new System.Windows.Forms.Button();
            this.lblxlsBgs = new System.Windows.Forms.Label();
            this.BtnBgs = new System.Windows.Forms.Button();
            this.dgwLines = new System.Windows.Forms.DataGridView();
            this.Line = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Color = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.From = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.To = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Direction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxSCN = new System.Windows.Forms.CheckBox();
            this.lblxlsSigClosN = new System.Windows.Forms.Label();
            this.btnSigClosN = new System.Windows.Forms.Button();
            this.checkBoxSC = new System.Windows.Forms.CheckBox();
            this.lblxlsSigClos = new System.Windows.Forms.Label();
            this.btnSigClos = new System.Windows.Forms.Button();
            this.checkBoxBGN = new System.Windows.Forms.CheckBox();
            this.lblxlsBgsN = new System.Windows.Forms.Label();
            this.BtnBgsN = new System.Windows.Forms.Button();
            this.checkBoxBG = new System.Windows.Forms.CheckBox();
            this.checkBoxLX = new System.Windows.Forms.CheckBox();
            this.checkBoxCmRts = new System.Windows.Forms.CheckBox();
            this.checkBoxRts = new System.Windows.Forms.CheckBox();
            this.checkBoxSpProf = new System.Windows.Forms.CheckBox();
            this.checkBoxEmSt = new System.Windows.Forms.CheckBox();
            this.checkBoxFP = new System.Windows.Forms.CheckBox();
            this.checkBoxDL = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblDocId = new System.Windows.Forms.Label();
            this.txtBoxDocId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBoxVersion = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblxlsRdd = new System.Windows.Forms.Label();
            this.btnRdd = new System.Windows.Forms.Button();
            this.checkBoxRdd = new System.Windows.Forms.CheckBox();
            this.cmbLines = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblAcSections = new System.Windows.Forms.Label();
            this.checkBoxAc = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.checkBoxLevel = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxR5 = new System.Windows.Forms.CheckBox();
            this.lblxlsOrderRdd = new System.Windows.Forms.Label();
            this.btnOrderRdd = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgwLines)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtBoxStationId
            // 
            this.txtBoxStationId.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtBoxStationId.Location = new System.Drawing.Point(349, 46);
            this.txtBoxStationId.Margin = new System.Windows.Forms.Padding(2);
            this.txtBoxStationId.MaxLength = 2;
            this.txtBoxStationId.Name = "txtBoxStationId";
            this.txtBoxStationId.Size = new System.Drawing.Size(93, 23);
            this.txtBoxStationId.TabIndex = 0;
            // 
            // lblStationId
            // 
            this.lblStationId.AutoSize = true;
            this.lblStationId.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStationId.Location = new System.Drawing.Point(4, 46);
            this.lblStationId.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStationId.Name = "lblStationId";
            this.lblStationId.Size = new System.Drawing.Size(82, 17);
            this.lblStationId.TabIndex = 1;
            this.lblStationId.Text = "Station Id:";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(776, 464);
            this.btnOK.Margin = new System.Windows.Forms.Padding(2);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 27);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblLine
            // 
            this.lblLine.AutoSize = true;
            this.lblLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblLine.Location = new System.Drawing.Point(4, 125);
            this.lblLine.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblLine.Name = "lblLine";
            this.lblLine.Size = new System.Drawing.Size(52, 17);
            this.lblLine.TabIndex = 4;
            this.lblLine.Text = "Lines:";
            // 
            // lblStationName
            // 
            this.lblStationName.AutoSize = true;
            this.lblStationName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStationName.Location = new System.Drawing.Point(4, 72);
            this.lblStationName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStationName.Name = "lblStationName";
            this.lblStationName.Size = new System.Drawing.Size(110, 17);
            this.lblStationName.TabIndex = 12;
            this.lblStationName.Text = "Station Name:";
            // 
            // txtBoxStationName
            // 
            this.txtBoxStationName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtBoxStationName.Location = new System.Drawing.Point(349, 73);
            this.txtBoxStationName.Margin = new System.Windows.Forms.Padding(2);
            this.txtBoxStationName.MaxLength = 256;
            this.txtBoxStationName.Name = "txtBoxStationName";
            this.txtBoxStationName.Size = new System.Drawing.Size(93, 23);
            this.txtBoxStationName.TabIndex = 11;
            // 
            // lblxlsDetLock
            // 
            this.lblxlsDetLock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsDetLock.Location = new System.Drawing.Point(99, 43);
            this.lblxlsDetLock.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsDetLock.Name = "lblxlsDetLock";
            this.lblxlsDetLock.Size = new System.Drawing.Size(260, 22);
            this.lblxlsDetLock.TabIndex = 18;
            this.lblxlsDetLock.Text = "Detection Locking";
            this.lblxlsDetLock.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnDetLock
            // 
            this.btnDetLock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDetLock.Location = new System.Drawing.Point(28, 43);
            this.btnDetLock.Margin = new System.Windows.Forms.Padding(2);
            this.btnDetLock.Name = "btnDetLock";
            this.btnDetLock.Size = new System.Drawing.Size(66, 22);
            this.btnDetLock.TabIndex = 17;
            this.btnDetLock.Text = "Browse";
            this.btnDetLock.UseVisualStyleBackColor = true;
            this.btnDetLock.Click += new System.EventHandler(this.BtnDetLock_Click);
            // 
            // lblxlsFP
            // 
            this.lblxlsFP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsFP.Location = new System.Drawing.Point(99, 70);
            this.lblxlsFP.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsFP.Name = "lblxlsFP";
            this.lblxlsFP.Size = new System.Drawing.Size(260, 22);
            this.lblxlsFP.TabIndex = 20;
            this.lblxlsFP.Text = "Flank Protection";
            this.lblxlsFP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnFP
            // 
            this.btnFP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFP.Location = new System.Drawing.Point(28MZê       ˇˇ  ∏       @                                   Ä   ∫ ¥	Õ!∏LÕ!This program cannot be run in DOS mode.
$       PE  L !aà[        ‡ " 0  *[  º     ûD[      `[   aV                      @_    q_  `Ö                           LD[ O    `[ π          Ë^ ¯D    _    C[                                                              H           .text   Ï([      *[                   `.rsrc   π  `[  ∫  ,[             @  @.reloc       _     Ê^             @  B                ÄD[     H      ≈% §⁄1 	       ƒüW –¢ îB[ Ä                                   {Ë 
*:(È 
}Ë 
* 0 #     u¥  
,(Í 
{Ë 
{Ë 
oÎ 
**v §Ù  )UU•Z(Í 
{Ë 
oÏ 
X*   0 M     r  pç$  %{Ë 

 ˛∂  å∂  -q∂  å∂  -&+˛∂  oÌ 
¢(Ó 
*ZsÔ 
( 
(Ò 
•∑  *(Ú 
*o} **   0 F       å∑  -#å∑  ,5å∑  ˛∑  oÛ 
- (  * å∑  ˛∑  oÛ 
-(  *  0 F       å∑  -#å∑  ,å∑  ˛∑  oÛ 
, (  * å∑  ˛∑  oÛ 
,(  *2,-(  *F,oÉ} -(  *Ç(Ù 
,(  oı 
oˆ 
-(  *:å∑  -(  *   0        ˛∑  å∑  ˛∑  oÛ 
&*   0        ˛∑  å∑  ˛∑  oÛ 
&*&,(  *&-(  *(  *:å∑  ,(  *:26(  *:22(  *N(˜ 
o¯ 
.(  *> (˘ 
-(  *> (˘ 
,(  *^(ä o˙ 
o˚ 
,(  *   0 %     3*Y
#ù^ L´π>4#ù^ L´πæ˛**F4(   ˛**F6(   ˛**:2(   **:0(   **R(¸ 
-
(˝ 
˛**j(%  ,#        ($  **ä(˛ 
Ä!  !  (ˇ 
!  (  
*⁄(˛ 
Ä"  "  #      ?[#      ?[(ˇ 
"  (  
*   0 >      ( 
 ( 
s 
('  
 ( 
 ( 
s 
('  s 
*  0 >      ( 
 ( 
s 
((  
 ( 
 ( 
s 
((  s 
*  0 A      ( 
 ( 
s 
('  
˛4   (	 
(
 
 ( 
( 
*   0 /      ( 
 ( 
s 
((  
 (	 
 ( 
s 
* 0 X      (9  (; s 
('  
 (=  (? s 
('   (	 
 ( 
(	 
( 
s, *"}#  *¶{#  0{#  s7  *{#   ˇˇ  _(8  *2q  (/  *.( 
s.  *0      y  {#  {#  ˛
ﬁ&
ﬁ *          5  0    	  {#  
 ( 
*>{#  {#  ˛*.(4  ˛* 0       s.  Ä$  s.  Ä%  s.  Ä&  s.  Ä'  s.  Ä(  s.  Ä)  s.  Ä*  s.  Ä+  s.  Ä,   s.  Ä-  Ws.  Ä.  zs.  Ä/   ◊   s.  Ä0   ˙  s.  Ä1   ê  s.  Ä2   ë  s.  Ä3   ∞  s.  Ä4   «  s.  Ä5   Ç  s.  Ä6     s.  Ä7  *"}C  *b-+    Äb``s7  *2{C  (:  *.c ˇ  _*2{C  (<  *" ˇˇ  _* 0 	  
  –
  ( 
o 

+Aöo 
–
  ( 
( 
,"o 
•
  	q
  (@  ,o 
*Xéi2π(9  3–  ( 
o 
+_öo 
–  ( 
( 
,:o 
•  (/  q
  (@  ,r=  po 
re  p( 
*Xéi2ô( 
ri  pç$  %{C  åç ¢(Ó 
*   0      y
  {C  {C  ˛
ﬁ&
ﬁ *          5  0      {C  
 ( 
*>{C  {C  ˛*.(@  ˛*6{C  ˛˛**{C  ˛*"(E  *   0 Æ     (C  9¢   (Ù 
,˛
  oÌ 
{C  (ˇ| 
o 
–  ( 
( 
,)(9  3(;  (G  
+M{C  s 

+>o 
ç^  %–Ö ( 
¢o 
( 
,ç$  %¢o 
u8  
z*  0      (1  (/  
 (D  *"s 
*0 4      s7  ÄD  s7  ÄE   
  Äs7  ÄF   @ Äs7  ÄG   @ Äs7  ÄH   @ Äs7  ÄI   @ Äs7  ÄJ   @ Äs7  ÄK   ˇˇ Äs7  ÄL    Äs7  ÄM   TÄs7  ÄN   Äs7  ÄO   Äs7  ÄP   Äs7  ÄQ    Äs7  ÄR    Äs7  ÄS   W Äs7  ÄT   Äs7  ÄU   "Äs7  ÄV   #Ó¿s7  ÄW   -Ó¿s7  ÄX  *r˛Q  så  ÄY  s 
ÄZ  *{_  *"}_  *  0     (  
}[  r{  p(! 
˛?  oÌ 
(" 
}\  ˛`  –`  ( 
(# 
}Ø }∞ ~Y  }± (Ã  }¥ (–  }∑ r£  p}∏ {\  }π 	
 (Ê  &˛>  ($ 
(% 
{\  (& 
i(' 
i(( 
i() 
i~* 
~* 
~* 
(´  (K  ﬁ(+ 
‹(, 
}^  *    ® Wˇ     0        (O  ﬁ(- 
‹*       

     >(O  (. 
*0 »     {]  ,*}]  (J  
{\  ,3{^  	˛P  s/ 
ç$  %~* 
åé ¢%¢o0 
&+U(J  ~* 
(1 
,C{^  o˚ 
,	(R  +-{^  	˛P  s/ 
ç$  %åé ¢%¢o0 
&~Z  o2 
&}˚r     ¸r     ˝r     ˛r     ˇr      s     s     s     s     s     s     s     	s     s     Ë˚	    s     s     s     s     s     s     s     >C
    s     s      s     "s     $s     &s     (s     *s     ,s     .s     0s     2s     4s     6s     8s     :s     <s     >s     @s     ¸	    Bs     Ds     Fs     Hs     Js     Ls     Ns     Ps     Rs     Ts     Vs     Xs     Zs     \s     ^s     `s     bs     ds     ì:
    °¸	    íC
    îC
    gs     is     ôC
    öC
    õC
    úC
    ùC
    ûC
    üC
    †C
    °C
    ¢C
    £C
    §C
    •C
    ¶C
    ßC
    ®C
    ©C
    ™C
    ´C
    ¨C
    ≠C
    ÆC
    ØC
    ±C
    ks     ≥C
    µC
    ns     ∑C
    πC
    Í∞    Î∞    rs     ts     »¸	    «¸	    …¸	     ¸	    À¸	    Õ¸	    Ã¸	    v	    ÀC
    ÃC
    ÕC
    œ¸	    Qì    ^ì    `ì    bì    zs     |s     ~s     Äs     Çs     Ñs     Üs     às     ÌC
    ¯C
    äs     ås     és     ês     ís     îs     ñs     í	    òs     ös     ús     ûs     †s     ¢s     §s     	   
 ¶s     ®s     ™s     ¨s     Æs     ∞s     ≤s     ¥s     ∂s     ∏s     ∫s     ºs     æs     ¿s     Ï	    √s     ∆s     …s     Ãs     œs     “s     ô    ’s     ◊s     Ÿs     €s     ›s     ﬂs     ·s     „s     è	    Âs     Ás     Ès     Îs     Ìs     Ôs     Òs     Ûs     ıs     ˜s     ˘s     ˚s     ˝s     ˇs     t     t     t     t     	t     t     t     t     t     t     t     t     t     t     t     t     "t     %t     +˝	    MD
    ,˝	    -˝	    't     )t     +t     .t     É˝	    Ñ˝	    *	    %	    -	    FÇ
    (	    `	    BÇ
    [	    Ç    ®                                                                                                      Ë∆    |8
    9
    ÚG
    ï8
    o    o    ¯     ˙     '     ø     ¿     r     +     O     l     ™"    ‡G
    ·G
    ‚G
    P     u    „G
    ‰G
    ÂG
    ÊG
    ä     ì     ∑     ∏     w	    ˇ◊    ,	    ˚◊    )	    ∫     á◊    	   
 £◊    _	    {	   	 ˜◊    ë	   
 t     u     ë     K     K     t     ä     [     y!     s    Nl    % \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   + \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 )   C \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O   H \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O \ 2 0 1 9   R \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O \ 2 0 1 9 \ C O M M U N I T Y   Z \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O \ 2 0 1 9 \ C O M M U N I T Y \ C O M M O N 7   ^ \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O \ 2 0 1 9 \ C O M M U N I T Y \ C O M M O N 7 \ I D E   o \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O \ 2 0 1 9 \ C O M M U N I T Y \ C O M M O N 7 \ I D E \ C O M M O N E X T E N S I O N S   y \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O \ 2 0 1 9 \ C O M M U N I T Y \ C O M M O N 7 \ I D E \ C O M M O N E X T E N S I O N S \ M I C R O S O F T   â \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O \ 2 0 1 9 \ C O M M U N I T Y \ C O M M O N 7 \ I D E \ C O M M O N E X T E N S I O N S \ M I C R O S O F T \ X A M L D I A G N O S T I C S   ì \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O \ 2 0 1 9 \ C O M M U N I T Y \ C O M M O N 7 \ I D E \ C O M M O N E X T E N S I O N S \ M I C R O S O F T \ X A M L D I A G N O S T I C S \ F R A M E W O R K   ó \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O \ 2 0 1 9 \ C O M M U N I T Y \ C O M M O N 7 \ I D E \ C O M M O N E X T E N S I O N S \ M I C R O S O F T \ X A M L D I A G N O S T I C S \ F R A M E W O R K \ X 6 4   ç \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S   ( X 8 6 ) \ M I C R O S O F T   V I S U A L   S T U D I O \ 2 0 1 9 \ C O M M U N I T Y \ C O M M O N 7 \ I D E \ C O M M O N E X T E N S I O N S \ M I C R O S O F T \ X A M L D I A G N O S T I C S \ X 6 4   . \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S \ A U T O D E S K   ; \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S \ A U T O D E S K \ A U T O C A D   2 0 2 0   A \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S \ A U T O D E S K \ A U T O C A D   2 0 2 0 \ E N - U S   A \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S \ A U T O D E S K \ A U T O C A D   2 0 2 0 \ S E T U P   C \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S \ A U T O D E S K \ A U T O C A D   2 0 2 0 \ S U P P O R T   , \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S \ L E N O V O   ? \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S \ L E N O V O \ B L U E T O O T H   S O F T W A R E   8 \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S \ N V I D I A   C O R P O R A T I O N   F \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M   F I L E S \ N V I D I A   C O R P O R A T I O N \ C O P R O C M A N A G E R   # \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M D A T A   6 \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M D A T A \ N V I D I A   C O R P O R A T I O N   : \ D E V I C E \ H A R D D I S K V O L U M E 2 \ P R O G R A M D A T A \ N V I D I A   C O R P O R A T I O N \ D R S    \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S   / \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V   7 \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A   = \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ L O C A L   F \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ L O C A L \ A U T O D E S K   S \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ L O C A L \ A U T O D E S K \ A U T O C A D   2 0 2 0   Y \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ L O C A L \ A U T O D E S K \ A U T O C A D   2 0 2 0 \ R 2 3 . 1   ] \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ L O C A L \ A U T O D E S K \ A U T O C A D   2 0 2 0 \ R 2 3 . 1 \ E N U   ? \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ R O A M I N G   H \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ R O A M I N G \ A U T O D E S K   U \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ R O A M I N G \ A U T O D E S K \ A U T O C A D   2 0 2 0   [ \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ R O A M I N G \ A U T O D E S K \ A U T O C A D   2 0 2 0 \ R 2 3 . 1   _ \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ R O A M I N G \ A U T O D E S K \ A U T O C A D   2 0 2 0 \ R 2 3 . 1 \ E N U   g \ D E V I C E \ H A R D D I S K V O L U M E 2 \ U S E R S \ G K A R P O V . A L C A T E L L V \ A P P D A T A \ R O A M I N G \ A U T O D E S K \ A U T O C A D   2 0 2 0 \ R 2 3 . 1 \ E N U \ S U P P O R T    \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S   ( \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y   C \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4   L \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ M S C O R L I B   m \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ M S C O R L I B \ D 6 0 0 9 E F B 3 2 E E E C F D D 2 F 5 8 5 5 C D 2 D 6 0 C 5 4   X \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ P R E S E N T A T I O 5 A E 0 F 0 0 F #   y \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ P R E S E N T A T I O 5 A E 0 F 0 0 F # \ D C C 0 8 D 0 C 8 5 A 4 3 F 3 5 1 D C D F 9 1 5 E B C 9 7 D A 4   T \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ P R E S E N T A T I O N C O R E   u \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ P R E S E N T A T I O N C O R E \ 0 5 D 6 B 4 F 6 1 E E 8 1 E E D D 1 3 4 0 2 8 4 6 B E 9 A A F C   J \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ S Y S T E M   O \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ S Y S T E M . C O R E   p \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ S Y S T E M . C O R E \ 6 4 F 3 C 7 2 0 A 1 E 9 C 6 D A A 4 1 F 8 7 6 8 C 8 B 7 D 4 8 B   O \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ S Y S T E M . X A M L   p \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ S Y S T E M . X A M L \ B 7 A A 6 F 6 2 C 8 0 7 C E 5 E C 7 F F 4 7 B 1 2 D A 5 7 F A A   k \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ S Y S T E M \ C 7 F F 0 E 7 2 5 B C 2 2 4 A 0 8 5 8 2 C 3 4 D F 5 2 5 A 4 6 6   O \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ W I N D O W S B A S E   p \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ A S S E M B L Y \ N A T I V E I M A G E S _ V 4 . 0 . 3 0 3 1 9 _ 6 4 \ W I N D O W S B A S E \ A 8 4 E F B 4 A 3 6 B E F 9 F A B 3 4 2 7 A 2 1 2 4 C 1 A B C 0   % \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ F O N T S   - \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ G L O B A L I Z A T I O N   5 \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ G L O B A L I Z A T I O N \ S O R T I N G   - \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ M I C R O S O F T . N E T   9 \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ M I C R O S O F T . N E T \ F R A M E W O R K 6 4   D \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ M I C R O S O F T . N E T \ F R A M E W O R K 6 4 \ V 4 . 0 . 3 0 3 1 9   K \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ M I C R O S O F T . N E T \ F R A M E W O R K 6 4 \ V 4 . 0 . 3 0 3 1 9 \ C O N F I G   ( \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ S Y S T E M 3 2   . \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ S Y S T E M 3 2 \ E N - U S   Ñ \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ W I N S X S \ A M D 6 4 _ M I C R O S O F T . W I N D O W S . C O M M O N - C O N T R O L S _ 6 5 9 5 B 6 4 1 4 4 C C F 1 D F _ 6 . 0 . 7 6 0 1 . 1 8 8 3 7 _ N O N E _ F A 3 B 1 E 3 D 1 7 5 9 4 7 5 7   | \ D E V I C E \ H A R D D I S K V O L U M E 2 \ W I N D O W S \ W I N S X S \ A M D 6 4 _ M I C R O S O F T . W I N D O W S . G D I P L U S _ 6 5 9 5 B 6 4 1 4 4 C C F 1 D F _ 1 . 1 . 7 6 0 1 . 2 4 3 0 8 _ N O N E _ 1 4 5 5 5 7 6 0 8 B 9 5 E 7 2 F                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               I t   m a y   a l s o   c o n t a i n 
     a d d i t i o n a l   d e v i c e   d r i v e r s   a n d   a p p l i c a t i o n   f i l e s . 
 
     W A R N I N G :   Y o u   w i l l   n o t   b e   p r o m p t e d   f o r   p e r m i s s i o n   t o   o v e r w r i t e   a n   e x i s t i n g   
                       l a n g . i n i   f i l e .   T h e   e x i s t i n g   l a n g . i n i   f i l e   w i l l   b e   o v e r w r i t t e n . 
 
         E x a m p l e s : 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / G e n - L a n g I n i   
                 / D i s t r i b u t i o n : D : \ d i s t r i b u t i o n 
 
                                S e t - I n p u t L o c a l e       PA            m S e t s   t h e   i n p u t   l o c a l e s   a n d   k e y b o a r d   l a y o u t s   t o   
                                                         u s e   i n   t h e   m o u n t e d   o f f l i n e   i m a g e .                   PAg
 / S e t - I n p u t L o c a l e : { < l o c a l e _ n a m e >   |   < l a n g u a g e _ i d > : < k e y b o a r d _ l a y o u t > }   
 
     S e t s   t h e   i n p u t   l o c a l e s   a n d   k e y b o a r d   l a y o u t s   t o   u s e   i n   t h e   m o u n t e d   o f f l i n e 
     i m a g e . 
     T h i s   c o m m a n d   i s   n o t   s u p p o r t e d   a g a i n s t   a n   o n l i n e   i m a g e . 
 
         E x a m p l e s : 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / S e t - I n p u t L o c a l e : e n - U S   
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / S e t - I n p u t L o c a l e : 0 4 0 9 : 0 0 0 0 0 4 0 9 
 
                               PA S e t - S y s L o c a l e                   Ø S e t s   t h e   l a n g u a g e   f o r   n o n - U n i c o d e   p r o g r a m s   ( a l s o 
                                                         c a l l e d   s y s t e m   l o c a l e )   a n d   f o n t   s e t t i n g s   i n   t h e   
                                                         m o u n t e d   o f f l i n e   i m a g e .                   Î
 / S e t - S y s L o c a l e : < l o c a l e _ n a m e > 
 
     S e t s   t h e   l a n g u a g e   f o r   n o n - U n i c o d e   p r o g r a m s   ( a l s o   c a l l e d   s y s t e m   l o c a l e )   a n d 
     f o n t   s e t t i n g s   i n   t h e   m o u n t e d   o f f l i n e   i m a g e . 
     T h i s   c o m m a n d   i s   n o t   s u p p o r t e d   a g a i n s t   a n   o n l i n e   i m a g e . 
 
     W A R N I N G :   Y o u   c a n n o t   s e t   U n i c o d e - o n l y   l a n g u a g e s   a s   t h e   s y s t e m   l o c a l e .   I f   y o u 
                       t r y ,   t h e   / S e t - S y s L o c a l e   o p t i o n   w i l l   f a i l   a n d   t h e   l a n g u a g e   f o r   
                       n o n - U n i c o d e   p r o g r a m s   w i l l   n o t   b e   c h a n g e d . 
 
         E x a m p l e : 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / S e t - S y s L o c a l e : e n - U S 
 
                       PA        
 S e t - U I L a n g                   j S e t s   t h e   d e f a u l t   s y s t e m   U I   l a n g u a g e   t h a t   i s   u s e d 
                                                         i n   t h e   m o u n t e d   o f f l i n e   i m a g e .                   S
 / S e t - U I L a n g : < l a n g u a g e _ n a m e > 
 
     S e t s   t h e   d e f a u l t   s y s t e m   u s e r   i n t e r f a c e   ( U I )   l a n g u a g e   t h a t   i s   u s e d   i n   t h e 
     m o u n t e d   o f f l i n e   i m a g e .   I f   t h e   l a n g u a g e   i s   n o t   i n s t a l l e d   i n   t h e   W i n d o w s   
     i m a g e ,   t h e   c o m m a n d   w i l l   f a i l . 
     T h i s   c o m m a n d   i s   n o t   s u p p o r t e d   a g a i n s t   a n   o n l i n e   i m a g e . 
 
         E x a m p l e : 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / S e t - U I L a n g : e n - U S 
 
               PA                 S e t - U I L a n g F a l l b a c k                   o S e t s   t h e   f a l l b a c k   d e f a u l t   l a n g u a g e   f o r   t h e   s y s t e m   
                                                         U I   i n   t h e   m o u n t e d   o f f l i n e   i m a g e .                   {
 / S e t - U I L a n g F a l l b a c k : < l a n g u a g e _ n a m e > 
 
     S e t s   t h e   f a l l b a c k   d e f a u l t   l a n g u a g e   f o r   t h e   s y s t e m   U I   i n   t h e   m o u n t e d   o f f l i n e 
     i m a g e .   T h i s   s e t t i n g   i s   u s e d   o n l y   w h e n   t h e   l a n g u a g e   s p e c i f i e d   b y   t h e   
     / S e t - U I L a n g   o p t i o n   i s   a   p a r t i a l l y   l o c a l i z e d   l a n g u a g e . 
     T h i s   c o m m a n d   i s   n o t   s u p p o r t e d   a g a i n s t   a n   o n l i n e   i m a g e . 
 
         E x a m p l e : 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / S e t - U I L a n g F a l l b a c k : e n - U S 
 
                                G e t - I n t l                   a D i s p l a y s   i n f o r m a t i o n   a b o u t   t h e   i n t e r n a t i o n a l   
                                                         s e t t i n g s   a n d   l a n g u a g e s .                   PA@
 / G e t - I n t l   [ / D i s t r i b u t i o n : < p a t h _ t o _ d i s t r i b u t i o n > ] 
 
     D i s p l a y s   i n f o r m a t i o n   a b o u t   i n t e r n a t i o n a l   s e t t i n g s   a n d   l a n g u a g e s . 
     U s e   t h e   / O n l i n e   o p t i o n   t o   d i s p l a y   i n f o r m a t i o n   a b o u t   i n t e r n a t i o n a l   s e t t i n g s 
     a n d   l a n g u a g e s   i n   t h e   r u n n i n g   o p e r a t i n g   s y s t e m . 
     U s e   / I m a g e   t o   d i s p l a y   i n f o r m a t i o n   a b o u t   i n t e r n a t i o n a l   s e t t i n g s   a n d   l a n g u a g e s 
     i n   t h e   o f f l i n e   i m a g e .     
     W h e n   u s e d   w i t h   t h e   / I m a g e   a n d   / D i s t r i b u t i o n   o p t i o n s ,   i n f o r m a t i o n   a b o u t 
     i n t e r n a t i o n a l   s e t t i n g s   a n d   l a n g u a g e s   i n   t h e   d i s t r i b u t i o n   i s   d i s p l a y e d . 
 
     W A R N I N G :   T h e   d e f a u l t   u s e r   l o c a l e   a n d   l o c a t i o n   i s   o n l y   r e p o r t e d 
     f o r   o f f l i n e   i m a g e s .   T h e   r e p o r t   d o e s   n o t   i n c l u d e   t h i s   s e t t i n g   f o r 
     r u n n i n g   o p e r a t i n g   s y s t e m s . 
 
         E x a m p l e s : 
             D I S M . e x e   / O n l i n e   / G e t - I n t l 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / G e t - I n t l 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / G e t - I n t l   / D i s t r i b u t i o n : D : \ d i s t r i b u t i o n 
 
                                S e t - U s e r L o c a l e                   2 S e t s   t h e   u s e r   l o c a l e   i n   t h e   m o u n t e d   o f f l i n e   i m a g e .                   ∏
 / S e t - U s e r L o c a l e : < l o c a l e _ n a m e > 
 
     S e t s   t h e   " s t a n d a r d s   a n d   f o r m a t s "   l a n g u a g e   ( a l s o   c a l l e d   u s e r   l o c a l e )   i n   t h e 
     m o u n t e d   o f f l i n e   i m a g e .   T h e   " S t a n d a r d s   a n d   f o r m a t s "   l a n g u a g e   i s   a 
     p e r - u s e r   s e t t i n g   t h a t   d e t e r m i n e s   d e f a u l t   s o r t   o r d e r   a n d   t h e   d e f a u l t   s e t t i n g s 
     f o r   f o r m a t t i n g   d a t e s ,   t i m e s ,   c u r r e n c y ,   a n d   n u m b e r s . 
     T h i s   c o m m a n d   i s   n o t   s u p p o r t e d   a g a i n s t   a n   o n l i n e   i m a g e . 
 
         E x a m p l e : 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / S e t U s e r L o c a l e : e n - U S 
 
                                S e t - T i m e Z o n e                   T S e t s   t h e   d e f a u l t   t i m e   z o n e   i n   t h e   m o u n t e d   o f f l i n e 
                                                         i m a g e .                   R
 / S e t - T i m e Z o n e : < t i m e z o n e _ n a m e > 
   
     S e t s   t h e   d e f a u l t   t i m e   z o n e   i n   a   W i n d o w s   i m a g e .   B e f o r e   s e t t i n g   t h e   t i m e   z o n e , 
     D I S M   v e r i f i e s   t h a t   t h e   s p e c i f i e d   t i m e   z o n e   s t r i n g   i s   v a l i d   f o r   t h e   i m a g e . 
     T h i s   c o m m a n d   i s   n o t   s u p p o r t e d   a g a i n s t   a n   o n l i n e   i m a g e . 
 
         E x a m p l e : 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / S e t - T i m e Z o n e : " W .   E u r o p e   S t a n d a r d   T i m e " 
 
                                S e t - S K U I n t l D e f a u l t s               PA    ™ S e t s   a l l   i n t e r n a t i o n a l   s e t t i n g s   t o   t h e   d e f a u l t 
                                                         v a l u e s   f o r   t h e   s p e c i f i e d   S K U   l a n g u a g e   i n   t h e 
                                                         m o u n t e d   o f f l i n e   i m a g e .                   ¿
 / S e t - S K U I n t l D e f a u l t s : < l a n g u a g e _ n a m e > 
   
     S e t s   t h e   d e f a u l t   s y s t e m   U I   l a n g u a g e ,   t h e   l a n g u a g e   f o r   n o n - U n i c o d e   p r o g r a m s , 
     t h e   " s t a n d a r d s   a n d   f o r m a t s "   l a n g u a g e   a n d   t h e   i n p u t   l o c a l e s ,   k e y b o a r d   
     l a y o u t s   a n d   t i m e   z o n e   v a l u e s   i n   a   m o u n t e d   o f f l i n e   i m a g e   t o   t h e 
     W i n d o w s   7   d e f a u l t   v a l u e   s p e c i f i e d   b y   < l a n g u a g e _ n a m e > . 
     T h i s   c o m m a n d   i s   n o t   s u p p o r t e d   a g a i n s t   a n   o n l i n e   i m a g e . 
 
         E x a m p l e : 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / S e t - S K U I n t l D e f a u l t s : e n - U S 
 
                                S e t - A l l I n t l       PA            Y S e t s   a l l   i n t e r n a t i o n a l   s e t t i n g s   i n   t h e   m o u n t e d 
                                                         o f f l i n e   i m a g e .                   PA 
 / S e t - A l l I n t l : < l a n g u a g e _ n a m e > 
 
     S e t s   t h e   d e f a u l t   s y s t e m   U I   l a n g u a g e ,   t h e   l a n g u a g e   f o r   n o n - U n i c o d e   p r o g r a m s , 
     t h e   " s t a n d a r d s   a n d   f o r m a t s "   l a n g u a g e   a n d   t h e   i n p u t   l o c a l e s   a n d   k e y b o a r d 
     l a y o u t s   t o   t h e   s p e c i f i e d   l a n g u a g e   i n   t h e   m o u n t e d   o f f l i n e   i m a g e . 
     I f   u s e d   w i t h   a n y   o f   t h e   o p t i o n s   u s e d   t o   s p e c i f y   t h e   i n d i v i d u a l   l a n g u a g e   o r 
     l o c a l e s ,   t h e n   t h e   i n d i v i d u a l   s e t t i n g s   t a k e   p r e c e d e n c e . 
     T h i s   c o m m a n d   i s   n o t   s u p p o r t e d   a g a i n s t   a n   o n l i n e   i m a g e . 
 
         E x a m p l e : 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / S e t - A l l I n t l : e n - U S 
 
                                S e t - L a y e r e d D r i v e r                    S e t s   k e y b o a r d   l a y e r e d   d r i v e r .                   
 / S e t - L a y e r e d D r i v e r : < l a y e r   I D > 
   
     S e t s   t h e   k e y b o a r d   l a y e r e d   d r i v e r ,   w i t h   p o s s i b l e 
     v a l u e s   o f   1   t o   6 ,   d e f i n e d   b e l o w : 
 
     1 :   P C / A T   E n h a n c e d   K e y b o a r d   ( 1 0 1 / 1 0 2 - K e y ) . 
     2 :   K o r e a n   P C / A T   1 0 1 - K e y   C o m p a t i b l e   K e y b o a r d / M S   N a t u r a l   K e y b o a r d   ( T y p e   1 ) . 
     3 :   K o r e a n   P C / A T   1 0 1 - K e y   C o m p a t i b l e   K e y b o a r d / M S   N a t u r a l   K e y b o a r d   ( T y p e   2 ) . 
     4 :   K o r e a n   P C / A T   1 0 1 - K e y   C o m p a t i b l e   K e y b o a r d / M S   N a t u r a l   K e y b o a r d   ( T y p e   3 ) . 
     5 :   K o r e a n   K e y b o a r d   ( 1 0 3 / 1 0 6   K e y ) . 
     6 :   J a p a n e s e   K e y b o a r d   ( 1 0 6 / 1 0 9   K e y ) . 
 
         E x a m p l e : 
             D I S M . e x e   / I m a g e : C : \ t e s t \ o f f l i n e   / S e t - L a y e r e d D r i v e r : 1 
 
                       PAƒ4   V S _ V E R S I O N _ I N F O     ΩÔ˛     Ø[±  Ø[±?                        "   S t r i n g F i l e I n f o   ˛   0 4 0 9 0 4 B 0   L   C o m p a n y N a m e     M i c r o s o f t   C o r p o r a t i o n   `   F i l e D e s c r i p t i o n     D I S M   I n t e r n a t i o n a l   P r o v i d e r   r )  F i l e V e r s i o n     6 . 1 . 7 6 0 1 . 2 3 4 7 1   ( w i n 7 s p 1 _ l d r . 1 6 0 6 1 4 - 0 6 0 0 )     B   I n t e r n a l N a m e   I n t l P r o v i d e r . d l l     Ä .  L e g a l C o p y r i g h t   ©   M i c r o s o f t   C o r p o r a t i o n .   A l l   r i g h t s   r e s e r v e d .   R   O r i g i n a l F i l e n a m e   I n t l P r o v i d e r . d l l . m u i     j %  P r o d u c t N a m e     M i c r o s o f t Æ   W i n d o w s Æ   O p e r a t i n g   S y s t e m     B   P r o d u c t V e r s i o n   6 . 1 . 7 6 0 1 . 2 3 4 7 1     D    V a r F i l e I n f o     $    T r a n s l a t i o n     	∞PADDINGXXPADDINGPADDINGXXPADDINGPADDINGXXPADDINGPADDINGXXPADDINGPADDINGXXPADDINGPADDINGXXPADDINGPADDINGXXPADDINGPADDINGXXPADDINGPADDING