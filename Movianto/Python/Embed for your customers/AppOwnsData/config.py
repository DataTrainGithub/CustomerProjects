# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.

class BaseConfig(object):

    # Can be set to 'MasterUser' or 'ServicePrincipal'
    AUTHENTICATION_MODE = 'ServicePrincipal'

    # Workspace Id in which the report is present
    WORKSPACE_ID = '570cf0be-b1b3-4da5-99c1-ea6f2ae37cf5'
    
    # Report Id for which Embed token needs to be generated
    REPORT_ID = '46d34045-7062-4e9d-9f16-b778310f7009'
    
    # Id of the Azure tenant in which AAD app and Power BI report is hosted. Required only for ServicePrincipal authentication mode.
    TENANT_ID = '536dbda2-f544-4dfc-9ae1-36dd34313862'

    # Client Id (Application Id) of the AAD app
    CLIENT_ID = '4ddb21e8-1cc3-4078-a2f1-4a41b71f543a'
    
    # Client Secret (App Secret) of the AAD app. Required only for ServicePrincipal authentication mode.
    CLIENT_SECRET = 'd4h8Q~gnzYIAA7RMu8xOIOAZvYVFiu.X2fN3pcK0'
    
    # Scope Base of AAD app. Use the below configuration to use all the permissions provided in the AAD app through Azure portal.
    SCOPE_BASE = ['https://analysis.windows.net/powerbi/api/.default']
    
    # URL used for initiating authorization request
    AUTHORITY_URL = 'https://login.microsoftonline.com/organizations'
    
    # Master user email address. Required only for MasterUser authentication mode.
    POWER_BI_USER = ''
    
    # Master user email password. Required only for MasterUser authentication mode.
    POWER_BI_PASS = ''