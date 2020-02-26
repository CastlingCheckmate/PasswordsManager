using Microsoft.Win32;
using System;
using System.Windows.Input;

using PasswordsManager.Cryptography;
using WPF.UI.Commands;
using WPF.UI.ViewModels;

namespace PasswordsManager.ViewModels
{

    public class MainViewModel : ViewModelBase
    {

        private ICommand _browseForInputFileCommand;
        private ICommand _browseForKeyFileCommand;
        private ICommand _browseForInitializationVectorFileCommand;
        private ICommand _loadCommand;
        private ICommand _selectCipherModeCommand;
        private ICommand _selectBlockSizeCommand;
        private ICommand _selectKeySizeCommand;
        private string _inputFileName;
        private string _keyFileName;
        private string _initializationVectorFileName;
        private RijndaelBlockSizes _blockSize;
        private RijndaelKeySizes _keySize;
        private SymmetricCipherModes _cipherMode;

        public ICommand BrowseForInputFileCommand =>
            _browseForInputFileCommand ?? (_browseForInputFileCommand = new RelayCommand(_ => BrowseForInputFile()));

        public ICommand BrowseForKeyFileCommand =>
            _browseForKeyFileCommand ?? (_browseForKeyFileCommand = new RelayCommand(_ => BrowseForKeyFile()));

        public ICommand BrowseForInitializationVectorFileCommand =>
            _browseForInitializationVectorFileCommand ?? (_browseForInitializationVectorFileCommand = new RelayCommand(_ => BrowseForInitializationVectorFile()));

        public ICommand LoadCommand =>
            _loadCommand ?? (_loadCommand = new RelayCommand(_ => Load(), _ => IsDataFilled));

        public ICommand SelectBlockSizeCommand =>
            _selectBlockSizeCommand ?? (_selectBlockSizeCommand = new RelayCommand(blockSize => SelectBlockSize((RijndaelBlockSizes)blockSize)));

        public ICommand SelectKeySizeCommand =>
            _selectKeySizeCommand ?? (_selectKeySizeCommand = new RelayCommand(keySize => SelectKeySize((RijndaelKeySizes)keySize)));

        public ICommand SelectCipherModeCommand =>
            _selectCipherModeCommand ?? (_selectCipherModeCommand = new RelayCommand(cipherMode => SelectCipherMode((SymmetricCipherModes)cipherMode)));

        public string InputFileName
        {
            get =>
                _inputFileName;

            set
            {
                _inputFileName = value;
                NotifyPropertyChanged(nameof(InputFileName));
            }
        }

        public string KeyFileName
        {
            get =>
                _keyFileName;

            set
            {
                _keyFileName = value;
                NotifyPropertyChanged(nameof(KeyFileName));
            }
        }

        public string InitializationVectorFileName
        {
            get =>
                _initializationVectorFileName;

            set
            {
                _initializationVectorFileName = value;
                NotifyPropertyChanged(nameof(InitializationVectorFileName));
            }
        }

        public RijndaelBlockSizes BlockSize
        {
            get =>
                _blockSize;

            set
            {
                _blockSize = value;
                NotifyPropertyChanged(nameof(BlockSize));
            }
        }

        public RijndaelKeySizes KeySize
        {
            get =>
                _keySize;

            set
            {
                _keySize = value;
                NotifyPropertyChanged(nameof(KeySize));
            }
        }

        public SymmetricCipherModes CipherMode
        {
            get =>
                _cipherMode;

            set
            {
                _cipherMode = value;
                if (CipherMode == SymmetricCipherModes.ElectronicCodeBook)
                {
                    InitializationVectorFileName = null;
                }
                NotifyPropertyChanged(nameof(CipherMode));
            }
        }

        private void BrowseForInputFile()
        {
            var openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.CurrentDirectory,
                CheckFileExists = true,
                Title = "Browsing for input file..."
            };
            if (!openFileDialog.ShowDialog().Value)
            {
                return;
            }
            InputFileName = openFileDialog.FileName;
        }

        private void BrowseForKeyFile()
        {
            var openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.CurrentDirectory,
                CheckFileExists = true,
                Title = "Browsing for key file...",
                Filter = "Rijndael key file (*.rijnkey)|*.rijnkey"
            };
            if (!openFileDialog.ShowDialog().Value)
            {
                return;
            }
            KeyFileName = openFileDialog.FileName;
        }

        private void BrowseForInitializationVectorFile()
        {
            var openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.CurrentDirectory,
                CheckFileExists = true,
                Title = "Browsing for IV file...",
                Filter = "Rijndael IV file (*.rijniv)|*.rijniv"
            };
            if (!openFileDialog.ShowDialog().Value)
            {
                return;
            }
            InitializationVectorFileName = openFileDialog.FileName;
        }

        private bool IsDataFilled =>
            !string.IsNullOrEmpty(InputFileName) &&
            !string.IsNullOrEmpty(KeyFileName) &&
            (!string.IsNullOrEmpty(InitializationVectorFileName) || CipherMode == SymmetricCipherModes.ElectronicCodeBook);

        public bool IsECBCipherModeSelected =>
            CipherMode == SymmetricCipherModes.ElectronicCodeBook;

        private void Load()
        {

        }

        private void SelectBlockSize(RijndaelBlockSizes blockSize)
        {
            BlockSize = blockSize;
        }

        private void SelectKeySize(RijndaelKeySizes keySize)
        {
            KeySize = keySize;
        }

        private void SelectCipherMode(SymmetricCipherModes cipherMode)
        {
            CipherMode = cipherMode;
        }

    }

}