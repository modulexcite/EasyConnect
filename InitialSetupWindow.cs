﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;
using EasyConnect.Protocols;

namespace EasyConnect
{
	public partial class InitialSetupWindow : Form
	{
		protected int _currentStep = 0;

		public InitialSetupWindow()
		{
			InitializeComponent();
		}

		protected MainForm MainForm
		{
			get
			{
				return Owner as MainForm;
			}
		}

		protected Form CurrentStepForm
		{
			get
			{
				return _containerPanel.Controls[0] as Form;
			}
		}

		protected SecureString SharingPassword
		{
			get;
			set;
		}

		protected bool UseSharedBookmarksFile
		{
			get;
			set;
		}

		protected string SharedBookmarksFilePath
		{
			get;
			set;
		}

		private void _nextButton_Click(object sender, EventArgs e)
		{
			if (!SaveCurrentStep(true))
				return;

			_currentStep++;
			SetNextStep();
		}

		public bool SaveCurrentStep(bool validate)
		{
			if (_currentStep == 1)
			{
				UseSharedBookmarksFile = (CurrentStepForm as BookmarksLocationForm).UseSharedBookmarksFile;
				SharedBookmarksFilePath = (CurrentStepForm as BookmarksLocationForm).SharedBookmarksFilePath;

				if (validate && UseSharedBookmarksFile && String.IsNullOrEmpty(SharedBookmarksFilePath))
				{
					MessageBox.Show(this, "Please enter the path to the shared bookmarks file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}

			else if (_currentStep == 2)
			{
				SharingPassword = (CurrentStepForm as IEncryptionConversionForm).SharingPassword;

				if (validate && (SharingPassword == null || SharingPassword.Length == 0))
				{
					MessageBox.Show(this, "Please enter the sharing password for the bookmarks file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}

			return true;
		}

		public void SetNextStep()
		{
			_previousButton.Enabled = _currentStep > 1;
			_nextButton.Enabled = _currentStep < 3;
			_finishButton.Enabled = _currentStep == 3;

			Form stepForm = null;

			if (_currentStep == 1)
			{
				stepForm = new BookmarksLocationForm();

				(stepForm as BookmarksLocationForm).UseSharedBookmarksFile = UseSharedBookmarksFile;
				(stepForm as BookmarksLocationForm).SharedBookmarksFilePath = SharedBookmarksFilePath;
			}

			else if (_currentStep == 2)
			{
				stepForm = MainForm.Options.EncryptionType == EncryptionType.Rijndael && (!UseSharedBookmarksFile || (UseSharedBookmarksFile && !File.Exists(SharedBookmarksFilePath)))
					           ? (Form) new RijndaelConversionForm()
					           : new RsaConversionForm();

				if (SharingPassword != null && SharingPassword.Length > 0)
					(stepForm as IEncryptionConversionForm).SharingPassword = SharingPassword;
			}

			else if (_currentStep == 3)
				stepForm = new FinishedForm();

			_containerPanel.Controls.Clear();

			stepForm.FormBorderStyle = FormBorderStyle.None;
			stepForm.TopLevel = false;
			stepForm.Parent = _containerPanel;
			stepForm.Show();
		}

		private void _finishButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void _previousButton_Click(object sender, EventArgs e)
		{
			SaveCurrentStep(false);

			_currentStep--;
			SetNextStep();
		}
	}
}
