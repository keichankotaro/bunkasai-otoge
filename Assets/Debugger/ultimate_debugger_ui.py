# -*- coding: utf-8 -*-
import dearpygui.dearpygui as dpg
import socket
import threading
import json
import time
import math
from collections import deque
import queue
import random

# --- 設定 (Configuration) ---
HOST = "0.0.0.0"
PORT = 50051
MAX_LOG_LINES = 100
CONNECTION_TIMEOUT = 5.0  # 接続が失われたと見なす秒数
GRAPH_DATA_POINTS = 200 # 速度グラフに表示するデータポイント数

# --- グローバル状態 (Global State) ---
ui_state = {
    "L1": {"data": "データ待機中...", "flash_timer": 0},
    "L2": {"data": "データ待機中...", "flash_timer": 0},
    "L3": {"data": "データ待機中...", "flash_timer": 0},
    "L4": {"data": "データ待機中...", "flash_timer": 0},
    "SpeedManager": {
        "data": "速度変更イベント待機中...",
        "flash_timer": 0,
        "current_speed": 0.0,
        "speed_history": deque([0.0] * GRAPH_DATA_POINTS, maxlen=GRAPH_DATA_POINTS),
        "time_history": deque(list(range(GRAPH_DATA_POINTS)), maxlen=GRAPH_DATA_POINTS),
        "bezier_params": None # (x1, y1, x2, y2)
    },
    "MusicInfo": {"data": {}},
    "connection_status": "切断",
    "last_message_time": 0,
    "log_messages": deque(maxlen=MAX_LOG_LINES),
    "time_counter": GRAPH_DATA_POINTS # グラフのX軸用
}

# スレッドセーフなキュー (Thread-safe queue)
message_queue = queue.Queue()
# フラッシュエフェクト用のテーマを保持する辞書 (Dictionary to hold themes)
flash_themes = {}

# --- ヘルパー関数 (Helper Functions) ---
def get_theme_colors(alpha):
    """UIのカラーパレットを定義"""
    return {
        "green": (0, 255, 100, alpha),
        "red": (255, 80, 80, alpha),
        "blue": (100, 150, 255, alpha),
        "orange": (255, 180, 100, alpha),
        "purple": (200, 120, 255, alpha),
        "yellow": (255, 255, 100, alpha),
        "cyan": (0, 255, 255, alpha),
        "cyan_fill": (0, 255, 255, 60),
        "white": (220, 220, 220, alpha),
        "dark_bg": (10, 12, 18, alpha),
        "light_bg": (25, 30, 40, alpha),
        "frame_bg": (20, 25, 35, alpha),
        "title_bg": (30, 60, 100, alpha),
    }

def format_note_display(payload):
    """レーン情報のテキスト表示をフォーマット"""
    try:
        note_type = payload.get("type", "不明")
        time_val = float(payload.get("time", 0.0))
        speed_val = float(payload.get("speed", 0.0))
    except (ValueError, TypeError):
        note_type = "データエラー"
        time_val = 0.0
        speed_val = 0.0
    
    display_str = f"種別: {note_type.upper()}\n"
    display_str += f"時間: {time_val:.3f}s\n"
    display_str += f"速度: x{speed_val:.2f}"
    return display_str

def format_speed_manager_display(payload):
    """スピード管理情報のテキスト表示をフォーマット"""
    event_type = payload.get("event", "不明なイベント")
    display_str = f"イベント: {event_type.replace('_', ' ').title()}\n"
    
    try:
        if event_type == "formula_update":
            duration = float(payload.get("duration", 0.0))
            progress = float(payload.get("t", 0.0))
            y1 = float(payload.get("y1", 0.0))
            y2 = float(payload.get("y2", 0.0))
            formula_str = f"f(t) = 3(1-t)²t({y1:.2f}) + 3(1-t)t²({y2:.2f}) + t³"
            display_str += f"期間: {duration:.2f}s | 進捗: {progress:.1%}\n"
            display_str += f"計算式: {formula_str}"
        elif event_type == "time_trigger":
            trigger_time = float(payload.get("trigger_time", 0.0))
            multiplier = float(payload.get("multiplier", 1.0))
            display_str += f"{trigger_time:.2f}sで発動 -> x{multiplier}"
        elif event_type == "formula_end":
            display_str += "計算式の変更が完了しました。"
        else:
            display_str += "速度は一定です。"
    except (ValueError, TypeError):
        display_str = "イベント: データ形式エラー"
        
    return display_str

def draw_bezier_curve(drawlist, width, height, params):
    """速度変更のベジェ曲線を描画"""
    if not params or width <= 40 or height <= 40:
        return
    
    x1, y1, x2, y2 = params
    p0 = [20, height - 20]
    p3 = [width - 20, 20]
    p1 = [p0[0] + (p3[0] - p0[0]) * x1, p0[1] - (p0[1] - p3[1]) * y1]
    p2 = [p0[0] + (p3[0] - p0[0]) * x2, p0[1] - (p0[1] - p3[1]) * y2]

    colors = get_theme_colors(150)
    dpg.draw_bezier_cubic(p1=p0, p2=p1, p3=p2, p4=p3, color=colors["yellow"], thickness=2, parent=drawlist)
    dpg.draw_line(p0, p1, color=colors["white"], thickness=1, parent=drawlist)
    dpg.draw_line(p3, p2, color=colors["white"], thickness=1, parent=drawlist)
    dpg.draw_circle(p1, radius=4, color=colors["red"], fill=colors["red"], parent=drawlist)
    dpg.draw_circle(p2, radius=4, color=colors["red"], fill=colors["red"], parent=drawlist)
    dpg.draw_text(f"P1({x1:.2f}, {y1:.2f})", pos=[p1[0]+5, p1[1]], color=colors["white"], parent=drawlist)
    dpg.draw_text(f"P2({x2:.2f}, {y2:.2f})", pos=[p2[0]+5, p2[1]], color=colors["white"], parent=drawlist)


# --- ソケットサーバーロジック (Socket Server Logic) ---
def socket_server_thread():
    """UDPパケットを待ち受け、キューに入れる"""
    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as s:
        s.bind((HOST, PORT))
        s.settimeout(1.0)
        print(f"ソケットサーバーが {HOST}:{PORT} で待機中")
        while dpg.is_dearpygui_running():
            try:
                data, addr = s.recvfrom(4096)
                message = data.decode('utf-8')
                message_queue.put(message)
            except socket.timeout:
                continue
            except Exception as e:
                error_msg = f"[エラー] ソケットエラー: {e}"
                message_queue.put(json.dumps({"source": "internal_error", "payload": error_msg}))
    print("ソケットサーバーのスレッドが停止しました。")

# --- データ処理 (Data Processing) ---
def process_received_data(message):
    """受信したJSONデータを処理し、UIの状態を更新"""
    try:
        data = json.loads(message)
        source = data.get("source", "Unknown")
        payload = data.get("payload", {})

        if source in ui_state:
            if isinstance(ui_state[source], dict):
                ui_state[source]["flash_timer"] = 1.0
                if source.startswith("L"):
                    formatted_payload = format_note_display(payload)
                elif source == "SpeedManager":
                    formatted_payload = format_speed_manager_display(payload)
                    try:
                        current_speed = float(payload.get("new_speed", ui_state["SpeedManager"]["current_speed"]))
                    except (ValueError, TypeError):
                        current_speed = ui_state["SpeedManager"]["current_speed"]
                    ui_state["SpeedManager"]["current_speed"] = current_speed
                    ui_state["SpeedManager"]["speed_history"].append(current_speed)
                    ui_state["SpeedManager"]["time_history"].append(ui_state["time_counter"])
                    ui_state["time_counter"] += 1
                    if payload.get("event") == "formula_update":
                        ui_state["SpeedManager"]["bezier_params"] = (
                            payload.get("x1", 0), payload.get("y1", 0),
                            payload.get("x2", 0), payload.get("y2", 0)
                        )
                    else:
                        ui_state["SpeedManager"]["bezier_params"] = None
                elif source == "MusicInfo":
                    ui_state["MusicInfo"]["data"] = payload
                    update_music_info_display(payload)
                    return
                else:
                    formatted_payload = json.dumps(payload, indent=2)
                ui_state[source]["data"] = formatted_payload
        
        elif source == "internal_error":
             add_log_message(payload, "red")
             return

        log_message = f"[{time.strftime('%H:%M:%S')}] {source}からのデータ"
        add_log_message(log_message, "cyan")
        
        ui_state["connection_status"] = "接続"
        ui_state["last_message_time"] = time.time()

    except json.JSONDecodeError:
        add_log_message(f"[エラー] 非JSONデータ: {message[:100]}...", "red")
    except Exception as e:
        add_log_message(f"[エラー] データ処理に失敗しました: {e}", "red")

def add_log_message(message, color_key):
    """イベントログに新しいメッセージを追加"""
    ui_state["log_messages"].appendleft((message, color_key))

def update_music_info_display(info):
    """楽曲情報パネルを更新"""
    title = info.get('title', 'N/A')
    composer = info.get('composer', 'N/A')
    level = info.get('level', 'N/A')
    
    dpg.set_value("music_title", f"曲名: {title}")
    dpg.set_value("music_composer", f"作曲者: {composer}")
    dpg.set_value("music_level", f"レベル: {level}")

# --- GUIセットアップ (GUI Setup) ---
def setup_gui():
    """メインGUIウィンドウとコンポーネントを作成"""
    dpg.create_context()
    
    # 日本語表示のためにフォントを読み込む
    with dpg.font_registry():
        try:
            # Windowsの一般的な日本語フォント(Meiryo UI)を、日本語の文字セットを含めて読み込む
            font = dpg.add_font("C:/Windows/Fonts/meiryo.ttc", 16)
            dpg.add_font_range_hint(dpg.mvFontRangeHint_Japanese, parent=font)
            dpg.bind_font(font)
            print("日本語フォント(Meiryo)を読み込みました。")
        except Exception as e:
            print(f"日本語フォント(Meiryo)を読み込めませんでした: {e}")
            try:
                # フォールバックとして別のフォント(Yu Gothic)を試す
                font = dpg.add_font("C:/Windows/Fonts/yugothr.ttc", 16)
                dpg.add_font_range_hint(dpg.mvFontRangeHint_Japanese, parent=font)
                dpg.bind_font(font)
                print("フォールバックフォント(Yu Gothic)を使用します。")
            except Exception as e2:
                print(f"フォールバックフォントも読み込めませんでした。文字化けが発生する可能性があります。: {e2}")


    colors = get_theme_colors(255)
    with dpg.theme() as global_theme:
        with dpg.theme_component(dpg.mvAll):
            dpg.add_theme_style(dpg.mvStyleVar_WindowRounding, 8)
            dpg.add_theme_style(dpg.mvStyleVar_FrameRounding, 6)
            dpg.add_theme_style(dpg.mvStyleVar_ChildRounding, 6)
            dpg.add_theme_style(dpg.mvStyleVar_GrabRounding, 4)
            dpg.add_theme_color(dpg.mvThemeCol_TitleBgActive, colors["title_bg"])
            dpg.add_theme_color(dpg.mvThemeCol_TitleBg, (colors["title_bg"][0], colors["title_bg"][1], colors["title_bg"][2], 150))
            dpg.add_theme_color(dpg.mvThemeCol_WindowBg, colors["dark_bg"])
            dpg.add_theme_color(dpg.mvThemeCol_ChildBg, colors["light_bg"])
            dpg.add_theme_color(dpg.mvThemeCol_Button, colors["title_bg"])
            dpg.add_theme_color(dpg.mvThemeCol_FrameBg, colors["frame_bg"])
            dpg.add_theme_color(dpg.mvThemeCol_Text, colors["white"])
            dpg.add_theme_color(dpg.mvPlotCol_Line, colors["cyan"])
            dpg.add_theme_color(dpg.mvPlotCol_Fill, colors["cyan_fill"])
            dpg.add_theme_color(dpg.mvPlotCol_PlotBg, (0,0,0,0))

    dpg.bind_theme(global_theme)
    
    # フラッシュエフェクト用のテーマを事前に作成
    for source in ["L1", "L2", "L3", "L4", "SpeedManager"]:
        with dpg.theme(tag=f"theme_{source}") as theme:
            with dpg.theme_component(dpg.mvChildWindow):
                dpg.add_theme_color(dpg.mvThemeCol_Border, (0,0,0,0), tag=f"flash_border_{source}")
                dpg.add_theme_style(dpg.mvStyleVar_ChildBorderSize, 2.0)
        flash_themes[source] = theme


    with dpg.window(tag="main_window"):
        # --- ヘッダー (Header) ---
        with dpg.group(horizontal=True):
            dpg.add_text("ステータス:")
            dpg.add_text("切断", tag="status_text", color=colors["red"])
            dpg.add_spacer(width=50)
            dpg.add_text(f"UDPサーバー {HOST}:{PORT}", color=colors["yellow"])

        dpg.add_separator()

        # --- 楽曲情報 (Music Info) ---
        with dpg.child_window(height=60, no_scrollbar=True):
             with dpg.group(horizontal=True):
                dpg.add_text("曲名: 楽曲情報待機中...", tag="music_title")
                dpg.add_spacer(width=20)
                dpg.add_text("作曲者: N/A", tag="music_composer")
                dpg.add_spacer(width=20)
                dpg.add_text("レベル: N/A", tag="music_level")

        # --- メインダッシュボード (Main Dashboard) ---
        # --- 上段パネル (Top Panels) ---
        with dpg.group(horizontal=True):
            create_lane_panel("L1", "レーン 1")
            create_lane_panel("L2", "レーン 2")
            create_lane_panel("L3", "レーン 3")
            create_lane_panel("L4", "レーン 4")
        
        dpg.add_separator()

        # --- 下段パネル (Bottom Panels) ---
        with dpg.group(horizontal=True):
            # --- スピード管理パネル (Speed Manager Panel) ---
            with dpg.child_window(width=800, label="スピード管理", tag="win_SpeedManager"):
                with dpg.group(horizontal=True):
                    # 左側: テキスト情報
                    with dpg.group(width=450):
                        with dpg.group(horizontal=True):
                            dpg.add_text("現在の速度:", color=colors["yellow"])
                            dpg.add_text("x 0.000", tag="speed_display", color=colors["yellow"])
                        dpg.add_text(ui_state["SpeedManager"]["data"], tag="text_SpeedManager")
                    
                    # 右側: ベジェ曲線
                    with dpg.group():
                        dpg.add_text("計算式の可視化", color=colors["yellow"])
                        with dpg.child_window(height=100, tag="bezier_canvas_parent"):
                             dpg.add_drawlist(width=-1, height=-1, tag="bezier_canvas")
                
                dpg.add_separator()

                # 速度グラフ
                with dpg.plot(label="速度履歴", height=-1, width=-1, tag="speed_plot"):
                    dpg.add_plot_axis(dpg.mvXAxis, label="時間", tag="speed_plot_x")
                    dpg.add_plot_axis(dpg.mvYAxis, label="速度倍率", tag="speed_plot_y")
                    dpg.add_area_series(list(ui_state["SpeedManager"]["time_history"]), list(ui_state["SpeedManager"]["speed_history"]), parent="speed_plot_y", tag="speed_series_area")
                    dpg.add_line_series(list(ui_state["SpeedManager"]["time_history"]), list(ui_state["SpeedManager"]["speed_history"]), parent="speed_plot_y", tag="speed_series_line")

            # --- イベントログパネル (Event Log Panel) ---
            with dpg.child_window(width=-1, label="イベントログ"):
                with dpg.group(tag="log_group"):
                    dpg.add_text("ロガーを初期化しました。", color=colors["yellow"])


    dpg.create_viewport(title='究極のリズムゲームデバッガーPro', width=1280, height=720)
    dpg.setup_dearpygui()
    dpg.show_viewport()
    dpg.set_primary_window("main_window", True)

def create_lane_panel(tag, label):
    """単一レーンのパネルを作成"""
    # 1280pxの幅に4つのパネルが収まるように調整
    with dpg.child_window(width=305, height=220, label=label, tag=f"win_{tag}"):
        dpg.add_text(ui_state[tag]["data"], tag=f"text_{tag}", wrap=280)

# --- メインループ (Main Loop) ---
def render_loop_update():
    """フレーム毎に呼ばれるメインの更新ループ"""
    global ui_state
    # キューからメッセージを処理
    while not message_queue.empty():
        message = message_queue.get()
        process_received_data(message)

    # 接続タイムアウトを確認
    if ui_state["connection_status"] == "接続" and (time.time() - ui_state["last_message_time"] > CONNECTION_TIMEOUT):
        ui_state["connection_status"] = "切断"
        add_log_message("[警告] 接続がタイムアウトしました。", "orange")

    # ステータステキストを更新
    status = ui_state["connection_status"]
    colors = get_theme_colors(255)
    color = colors["green"] if status == "接続" else colors["red"]
    dpg.set_value("status_text", status)
    dpg.configure_item("status_text", color=color)

    # UI要素を更新
    for source, state in ui_state.items():
        if not isinstance(state, dict):
            continue

        if source.startswith("L"):
            dpg.set_value(f"text_{source}", state["data"])
        
        elif source == "SpeedManager":
            dpg.set_value("text_SpeedManager", state["data"])
            dpg.set_value("speed_display", f"x {state['current_speed']:.3f}")
            # グラフデータを更新
            graph_data = [list(state["time_history"]), list(state["speed_history"])]
            dpg.set_value("speed_series_area", graph_data)
            dpg.set_value("speed_series_line", graph_data)

            if state["time_history"]:
                # Y軸の自動調整
                min_val = min(state["speed_history"])
                max_val = max(state["speed_history"])
                
                if min_val == max_val:
                    final_min = max(0, min_val - 1)
                    final_max = max_val + 1
                else:
                    padding = (max_val - min_val) * 0.1
                    final_min = max(0, min_val - padding)
                    final_max = max_val + padding
                
                dpg.set_axis_limits("speed_plot_y", final_min, final_max)
                dpg.set_axis_limits("speed_plot_x", state["time_history"][0], state["time_history"][-1])

            dpg.delete_item("bezier_canvas", children_only=True)
            if state["bezier_params"]:
                width = dpg.get_item_width("bezier_canvas_parent")
                height = dpg.get_item_height("bezier_canvas_parent")
                if width and height:
                    draw_bezier_curve("bezier_canvas", width, height, state["bezier_params"])

        # フラッシュエフェクトとタイマーの処理
        if "flash_timer" in state and state["flash_timer"] > 0:
            state["flash_timer"] -= dpg.get_delta_time()
            flash_alpha = int(150 * state["flash_timer"])
            if source in flash_themes and dpg.does_item_exist(f"win_{source}"):
                color_key = {"L1":"green", "L2":"blue", "L3":"orange", "L4":"purple", "SpeedManager":"yellow"}[source]
                base_color = colors[color_key]
                flash_color = (base_color[0], base_color[1], base_color[2], flash_alpha)
                dpg.set_value(f"flash_border_{source}", flash_color)
                dpg.bind_item_theme(f"win_{source}", flash_themes[source])
        elif "flash_timer" in state: # タイマーが切れた
            if source in flash_themes and dpg.does_item_exist(f"win_{source}"):
                dpg.bind_item_theme(f"win_{source}", 0)


    # ログを更新
    if dpg.does_item_exist("log_group"):
        dpg.delete_item("log_group", children_only=True)
        for msg, c_key in ui_state["log_messages"]:
            dpg.add_text(msg, color=colors[c_key], parent="log_group", wrap=450)


if __name__ == "__main__":
    setup_gui()
    server_thread = threading.Thread(target=socket_server_thread, daemon=True)
    server_thread.start()
    
    while dpg.is_dearpygui_running():
        render_loop_update()
        dpg.render_dearpygui_frame()
        
    dpg.destroy_context()
