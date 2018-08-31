namespace TaskServiceManage
{
	partial class Main
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.btnInstall = new System.Windows.Forms.Button();
			this.btnStart = new System.Windows.Forms.Button();
			this.btnEnd = new System.Windows.Forms.Button();
			this.btnUnInstall = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnInstall
			// 
			this.btnInstall.Location = new System.Drawing.Point(21, 34);
			this.btnInstall.Name = "btnInstall";
			this.btnInstall.Size = new System.Drawing.Size(108, 43);
			this.btnInstall.TabIndex = 0;
			this.btnInstall.Text = "安装服务";
			this.btnInstall.UseVisualStyleBackColor = true;
			this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(181, 34);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(108, 43);
			this.btnStart.TabIndex = 1;
			this.btnStart.Text = "启动任务";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// btnEnd
			// 
			this.btnEnd.Location = new System.Drawing.Point(341, 34);
			this.btnEnd.Name = "btnEnd";
			this.btnEnd.Size = new System.Drawing.Size(108, 43);
			this.btnEnd.TabIndex = 2;
			this.btnEnd.Text = "停止任务";
			this.btnEnd.UseVisualStyleBackColor = true;
			this.btnEnd.Click += new System.EventHandler(this.btnEnd_Click);
			// 
			// btnUnInstall
			// 
			this.btnUnInstall.Location = new System.Drawing.Point(505, 34);
			this.btnUnInstall.Name = "btnUnInstall";
			this.btnUnInstall.Size = new System.Drawing.Size(108, 43);
			this.btnUnInstall.TabIndex = 3;
			this.btnUnInstall.Text = "卸载服务";
			this.btnUnInstall.UseVisualStyleBackColor = true;
			this.btnUnInstall.Click += new System.EventHandler(this.btnUninstall_Click);
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(640, 114);
			this.Controls.Add(this.btnUnInstall);
			this.Controls.Add(this.btnEnd);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.btnInstall);
			this.Name = "Main";
			this.Text = "任务服务管理";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnInstall;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnEnd;
		private System.Windows.Forms.Button btnUnInstall;
	}
}

