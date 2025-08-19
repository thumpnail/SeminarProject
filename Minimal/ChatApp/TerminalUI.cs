namespace ChatApp;

using System.Collections;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using System.Collections.ObjectModel;
using Terminal.Gui;

public sealed class ChatWindow : Window {
	const int CHAT_WINDOW_HEIGHT = 3;

	// Components
	private static ListView _roomsListView;
	private static ListView _chatListView;
	private static ListView _userListView;
	private static TextField _chatMessagePromptView;

	// Data
	private readonly List<string> _messages = [];
	private readonly HashSet<string> _users = [];
	private readonly HashSet<string> _rooms = ["Admins", "Users"];

	// Services
	// private readonly SignalRClient signalRClient;
	// private readonly ChatService chatService;

	public ChatWindow() {
		// Services
		//this.signalRClient = signalRClient;
		//this.chatService = chatService;

		// Window
		X = 0;
		Y = 1;
		Width = Dim.Fill();
		Height = Dim.Fill();

		Setup();

		// SignalR

		// Initialize data asynchronously
		_ = InitializeAsync();
	}

	private async Task InitializeAsync() {
		try {
			// TODO: For future implementation
			//var initialMessages = await chatService.GetMessagesAsync();
			var activeRooms = new List<string>(); //await signalRClient.connection.InvokeAsync<HashSet<string>>("ShowActiveRooms");
			var activeUsers = new List<string>(); // await signalRClient.connection.InvokeAsync<HashSet<string>>("ShowActiveUsers");

			// Update UI sources
			Application.Invoke(() => {
				_chatListView.SetSource(new ObservableCollection<string>(_messages));

				_rooms.Clear();
				foreach (var room in activeRooms) {
					_rooms.Add(room);
				}

				_roomsListView.SetSource(new ObservableCollection<string>(_rooms));

				_users.Clear();
				foreach (var user in activeUsers) {
					_users.Add(user);
				}
				_userListView.SetSource(new ObservableCollection<string>(_users));
			});
		} catch (Exception ex) {
			MessageBox.ErrorQuery(40, 10, "Error", ex.Message, "Ok");
		}
	}

	#region Owner interface

	public void Setup() {
		// Rooms
		var roomsListFrame = new FrameView {
			Title = "Rooms",
			X = 0,
			Y = 0,
			Width = Dim.Percent(20),
			Height = Dim.Fill(),
		};

		_roomsListView = new ListView {
			X = 0,
			Y = 0,
			Width = Dim.Fill(),
			Height = Dim.Fill(),
		};

		roomsListFrame.Add(_roomsListView);
		Add(roomsListFrame);

		// Chat
		var chatFrameView = new FrameView {
			Title = "Chat",
			X = Pos.Right(roomsListFrame),
			Y = 0,
			Width = Dim.Percent(60),
			Height = Dim.Fill() - CHAT_WINDOW_HEIGHT,
		};

		_chatListView = new ListView {
			X = 0,
			Y = 0,
			Width = Dim.Fill(),
			Height = Dim.Fill(),
		};

		chatFrameView.Add(_chatListView);
		Add(chatFrameView);

		// Users
		var userListFrame = new FrameView {
			Title = "Users",
			X = Pos.Right(chatFrameView),
			Y = 0,
			Width = Dim.Percent(20),
			Height = Dim.Fill(),
		};

		_userListView = new ListView() {
			Width = Dim.Fill(),
			Height = Dim.Fill(),
		};

		userListFrame.Add(_userListView);
		Add(userListFrame);

		var chatBar = new FrameView {
			Title = "Message",
			X = Pos.Right(roomsListFrame),
			Y = Pos.Bottom(chatFrameView),
			Width = Dim.Percent(60),
			Height = CHAT_WINDOW_HEIGHT
		};

		_chatMessagePromptView = new TextField {
			X = 0,
			Y = Pos.Center(),
			Width = Dim.Fill(),
			Height = Dim.Fill()
		};

		// Test
		_userListView.SetSource(new ObservableCollection<string>(_users));

		_chatMessagePromptView.KeyDown += (_, a) => {
			if (a.KeyCode == Key.Enter) {
				string message = _chatMessagePromptView.Text.ToString();
				if (!string.IsNullOrEmpty(message) && message[0] == '/') {
					ExecuteCommand(message);
					_chatMessagePromptView.Text = string.Empty;
					a.Handled = true;
				} else {
					AddMessageToChat("You", message);
					//signalRClient.connection.InvokeCoreAsync("SendGroupMessageAsync", args: [message]);
					_chatMessagePromptView.Text = string.Empty;
					a.Handled = true;
				}
			}
		};

		chatBar.Add(_chatMessagePromptView);
		Add(chatBar);
	}

	public MenuBar CreateMenuBar() {
		// TODO: Add menu bar
		return new MenuBar {
			Title = "TestMenuBar",
			Menus = [
				new MenuBarItem("_App", new MenuItem[] {
					new MenuItem("_Quit", "", () => Application.RequestStop(), null, null)
				})
			]
		};
	}

	#endregion

	private async Task HandleConnectionStatusChangeAsync() {
		try {
			var activeUsers = await FetchActiveUsersAsync();
			_userListView.SetSource(new ObservableCollection<string>(activeUsers));
		}
		catch (Exception ex) {
			// Handle exceptions appropriately
			Console.WriteLine($"Error during connection status change: {ex.Message}");
		}
	}

	private async Task<HashSet<string>> FetchActiveUsersAsync() {
		try {
			return new();
			//return await signalRClient.connection.InvokeAsync<HashSet<string>>("ShowActiveUsers");
		}
		catch (Exception ex) {
			Console.WriteLine($"Error fetching active users: {ex.Message}");
			return new HashSet<string>();
		}
	}

	public void ExecuteCommand(string command) {
		switch (command) {
			case "/clear":
				_messages.Clear();
				_chatListView.MovePageUp();
				break;
			default:
				break;
		}
	}

	private void AddMessageToChat(string user, string message) {
		_messages.Add($"{user}: {message ?? ""}");
		_chatListView.SetSource(new ObservableCollection<string>(_messages));
		_chatListView.MoveEnd();
		//Application.Refresh();
	}
}
