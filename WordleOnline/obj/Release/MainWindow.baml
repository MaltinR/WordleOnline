   M S B A M L   `   `   `  ���� G  CWordleOnline, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:WordleOnlineWordleOnline      WordleOnline.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:WordleOnline     -     �Name$  Window�� � Title$ Wordle Online by MartinL��$	��450��$	��800����.   � 
ResizeMode$ CanMinimize=� � Icon$ Icon.png��.���� -    ��Name$ 
mainCanvas��|�. .+��� -   $ InputCanvas��$��Bottom=�$��Right=� �� Left$	 335��$	��449��$��50��
 �� Top$	 369��.+��� -    ��Name$ Connect��$��Connect��$��4��$��20��$��5��$��75��$��Bottom=�$��Right=�$	 370��$ 0��|�.  �� -   $
 Host��$
��Host��$��4��$��20��$��5��$��75��$��Bottom=�$��Right=�$	 370��$	 -25��|�.  �� -   $ 
Disconnect��$��	Disonnect��$��4��$��20��$��5��$��75��$��Bottom=�$��Right=�$ 0��$ 0��|�.  �� -   $ NewGame��$	��New��$��4��$��20��$��5��$��75��$��Bottom=�$��Right=�$ 0��$	 -25��|�.  �� -    ��Name$
 Name��$��20��$ 80��	 �� TextWrapping$
	 Wrap=�$
��Name��$ 0��$��70���� -	   $ 	IPAddress��$��20��$	 155��$
	 Wrap=�$��
IP Address��$ 0��$	��130���� -
   $
 Port��$��20��$	 290��$
	 Wrap=�$
��Port��$ 0��$��75���� -   
 ��Name$
 
statusText��$ 0��$
��Wrap=�$��Status: Not Connected��
���   �$ 25��$	��440���� $	��240��$	 352��$ 0��$	��440��.+�� -    �Name$ 
OnlineGrid��$	��240��$	��450��