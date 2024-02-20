# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.

from services.pbiembedservice import PbiEmbedService
from utils import Utils
from flask import Flask, request, render_template, send_from_directory
import json
import os

# Initialize the Flask app
app = Flask(__name__)

# Load configuration
app.config.from_object('config.BaseConfig') 

@app.route('/', methods = ['GET'])
def index():
    '''Returns a static HTML page'''
    TrackingCode = request.args.get('TrackingCode')
    print("HTML Tracking code = " + global_tracking_code)
    return render_template('index.html', TrackingCode=TrackingCode)

@app.route('/getembedinfo', methods=['GET'])
def get_embed_info():
    '''Returns report embed configuration'''

    config_result = Utils.check_config(app)
    if config_result is not None:
        return json.dumps({'errorMsg': config_result}), 500

    try:
        global global_tracking_code
        embed_info_json = PbiEmbedService().get_embed_params_for_single_report(
            app.config['WORKSPACE_ID'], 
            app.config['REPORT_ID']
        )
        embed_info = json.loads(embed_info_json)  # Parse as JSON
        print(embed_info)  # Print the entire embed_info dictionary
        return json.dumps(embed_info)
    except Exception as ex:
        return json.dumps({'errorMsg': str(ex)}), 500

@app.route('/favicon.ico', methods=['GET'])
def getfavicon():
    '''Returns path of the favicon to be rendered'''

    return send_from_directory(os.path.join(app.root_path, 'static'), 'img/favicon.ico', mimetype='image/vnd.microsoft.icon')

if __name__ == '__main__':
    app.run()