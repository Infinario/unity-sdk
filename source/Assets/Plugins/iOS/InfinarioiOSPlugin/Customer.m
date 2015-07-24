//
//  Customer.m
//  InfinarioSDK
//
//  Created by Igi on 2/5/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import "Customer.h"

@interface Customer ()

@property NSDictionary *ids;
@property NSString *projectId;
@property NSDictionary *properties;

@end

@implementation Customer

- (instancetype)initWithIds:(NSDictionary *)ids andProjectId:(NSString *)projectId andWithProperties:(NSDictionary *)properties {
    self = [super initWithEndpoint:@"crm/customers"];
    
    self.ids = ids;
    self.projectId = projectId;
    self.properties = properties;
    
    return self;
}

- (NSMutableDictionary *)getData {
    NSMutableDictionary *data = [[NSMutableDictionary alloc] init];
    
    data[@"ids"] = self.ids;
    data[@"project_id"] = self.projectId;
    
    if (self.properties) {
        data[@"properties"] = self.properties;
    }
    
    return data;
}

@end
